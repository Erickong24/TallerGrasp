namespace SubsidiosVivienda.WinForms
{
    internal class SubsidioAplicado
    {
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int Estrato { get; set; }
        public decimal IngresoMensual { get; set; }
        public bool CasaPropia { get; set; }
        public decimal SubsidioAsignado { get; set; }
    }
}
