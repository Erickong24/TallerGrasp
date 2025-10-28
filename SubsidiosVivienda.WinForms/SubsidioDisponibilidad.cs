using System;

namespace SubsidiosVivienda.WinForms
{
    internal class SubsidioDisponibilidad
    {
        public int Estrato { get; }
        public decimal ValorDisponible { get; set; }
        public decimal ValorSubsidio { get; }

        public SubsidioDisponibilidad(int estrato, decimal valorDisponible, decimal valorSubsidio)
        {
            if (estrato < 1 || estrato > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(estrato));
            }

            Estrato = estrato;
            ValorDisponible = valorDisponible;
            ValorSubsidio = valorSubsidio;
        }
    }
}
