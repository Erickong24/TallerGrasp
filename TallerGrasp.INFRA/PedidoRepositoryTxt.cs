using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TallerGrasp.ENT;

namespace TallerGrasp.INFRA
{
    public class PedidoRepositoryTxt : IPedidoRepository
    {
        private readonly string _filePath;
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        public PedidoRepositoryTxt(string? filePath = null)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pedidos.txt")
                : Path.GetFullPath(filePath);

            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, string.Empty);
            }
        }

        public IEnumerable<Pedido> GetAll()
        {
            if (!File.Exists(_filePath)) yield break;

            foreach (var line in File.ReadAllLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length < 4) continue;

                if (!int.TryParse(parts[0], out var id)) continue;
                var estudiante = parts[1].Trim();
                var libro = parts[2].Trim();

                DateTime fecha;
                var fechaStr = parts[3].Trim();
                if (!DateTime.TryParseExact(fechaStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    if (!DateTime.TryParse(fechaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                    {
                        fecha = DateTime.MinValue;
                    }
                }

                yield return new Pedido
                {
                    Id = id,
                    Estudiante = estudiante,
                    Libro = libro,
                    Fecha = fecha
                };
            }
        }

        public void Save(Pedido pedido)
        {
            var line = string.Join("|",
                pedido.Id.ToString(CultureInfo.InvariantCulture),
                pedido.Estudiante,
                pedido.Libro,
                pedido.Fecha.ToString(DateFormat, CultureInfo.InvariantCulture));

            File.AppendAllText(_filePath, line + Environment.NewLine);
        }

        public int GetMaxId()
        {
            int maxId = 0;
            if (!File.Exists(_filePath)) return 0;

            foreach (var line in File.ReadAllLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length == 0) continue;
                if (int.TryParse(parts[0], out var id))
                {
                    if (id > maxId) maxId = id;
                }
            }

            return maxId;
        }
    }
}

