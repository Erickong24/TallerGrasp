using System;

namespace TallerGrasp.ENT
{
    public class Pedido
    {
        public int Id { get; set; }
        public string Estudiante { get; set; } = string.Empty;
        public string Libro { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}

