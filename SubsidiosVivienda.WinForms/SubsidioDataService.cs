using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SubsidiosVivienda.WinForms
{
    internal class SubsidioDataService
    {
        private readonly string _archivoDisponibles;
        private readonly string _archivoAplicados;
        private readonly Dictionary<int, SubsidioDisponibilidad> _disponibilidades = new();

        private static readonly SubsidioDisponibilidad[] Inicial =
        {
            new(1, 500_000_000m, 50_000_000m),
            new(2, 400_000_000m, 40_000_000m),
            new(3, 300_000_000m, 30_000_000m),
            new(4, 100_000_000m, 10_000_000m)
        };

        public SubsidioDataService(string? baseDirectory = null)
        {
            var baseDir = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            _archivoDisponibles = Path.Combine(baseDir, "SubsidiosDisponibles.txt");
            _archivoAplicados = Path.Combine(baseDir, "SubsidiosAplicados.txt");

            AsegurarArchivos();
            CargarDisponibilidades();
        }

        public IReadOnlyCollection<SubsidioDisponibilidad> ObtenerDisponibilidades() => _disponibilidades.Values.ToList();

        public decimal ObtenerValorDisponible(int estrato)
        {
            if (!_disponibilidades.TryGetValue(estrato, out var disponibilidad))
            {
                throw new InvalidOperationException($"No hay configuración para el estrato {estrato}.");
            }

            return disponibilidad.ValorDisponible;
        }

        public decimal ObtenerSubsidio(int estrato)
        {
            if (!_disponibilidades.TryGetValue(estrato, out var disponibilidad))
            {
                throw new InvalidOperationException($"No hay configuración para el estrato {estrato}.");
            }

            return disponibilidad.ValorSubsidio;
        }

        public IReadOnlyList<SubsidioAplicado> LeerAplicados()
        {
            var resultado = new List<SubsidioAplicado>();
            if (!File.Exists(_archivoAplicados))
            {
                return resultado;
            }

            var lineas = File.ReadAllLines(_archivoAplicados);
            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                var partes = linea.Split(';');
                if (partes.Length < 6)
                {
                    continue;
                }

                if (!int.TryParse(partes[2], out int estrato))
                {
                    continue;
                }

                if (!decimal.TryParse(partes[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal ingreso))
                {
                    continue;
                }

                if (!decimal.TryParse(partes[5], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal subsidio))
                {
                    continue;
                }

                resultado.Add(new SubsidioAplicado
                {
                    Cedula = partes[0],
                    NombreCompleto = partes[1],
                    Estrato = estrato,
                    IngresoMensual = ingreso,
                    CasaPropia = partes[4].Trim().Equals("SI", StringComparison.OrdinalIgnoreCase),
                    SubsidioAsignado = subsidio
                });
            }

            return resultado;
        }

        public void GuardarAplicacion(SubsidioAplicado aplicado)
        {
            var linea = string.Join(';', new[]
            {
                aplicado.Cedula,
                aplicado.NombreCompleto,
                aplicado.Estrato.ToString(),
                aplicado.IngresoMensual.ToString(CultureInfo.InvariantCulture),
                aplicado.CasaPropia ? "SI" : "NO",
                aplicado.SubsidioAsignado.ToString(CultureInfo.InvariantCulture)
            });

            File.AppendAllLines(_archivoAplicados, new[] { linea });
        }

        public void DescontarSubsidio(int estrato)
        {
            if (!_disponibilidades.TryGetValue(estrato, out var disponibilidad))
            {
                throw new InvalidOperationException($"No existe disponibilidad para estrato {estrato}.");
            }

            disponibilidad.ValorDisponible -= disponibilidad.ValorSubsidio;
            if (disponibilidad.ValorDisponible < 0)
            {
                disponibilidad.ValorDisponible = 0;
            }

            GuardarDisponibilidades();
        }

        private void CargarDisponibilidades()
        {
            _disponibilidades.Clear();
            var lineas = File.ReadAllLines(_archivoDisponibles);
            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                var partes = linea.Split(';');
                if (partes.Length < 3)
                {
                    continue;
                }

                if (!int.TryParse(partes[0], out int estrato))
                {
                    continue;
                }

                if (!decimal.TryParse(partes[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valorDisponible))
                {
                    continue;
                }

                if (!decimal.TryParse(partes[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valorSubsidio))
                {
                    continue;
                }

                _disponibilidades[estrato] = new SubsidioDisponibilidad(estrato, valorDisponible, valorSubsidio);
            }
        }

        private void GuardarDisponibilidades()
        {
            var lineas = _disponibilidades.Values
                .OrderBy(d => d.Estrato)
                .Select(d => string.Join(';', new[]
                {
                    d.Estrato.ToString(),
                    d.ValorDisponible.ToString(CultureInfo.InvariantCulture),
                    d.ValorSubsidio.ToString(CultureInfo.InvariantCulture)
                }));

            File.WriteAllLines(_archivoDisponibles, lineas);
        }

        private void AsegurarArchivos()
        {
            if (!File.Exists(_archivoDisponibles))
            {
                var lineas = Inicial.Select(d => string.Join(';', new[]
                {
                    d.Estrato.ToString(),
                    d.ValorDisponible.ToString(CultureInfo.InvariantCulture),
                    d.ValorSubsidio.ToString(CultureInfo.InvariantCulture)
                }));
                File.WriteAllLines(_archivoDisponibles, lineas);
            }

            if (!File.Exists(_archivoAplicados))
            {
                using var _ = File.Create(_archivoAplicados);
            }
        }
    }
}
