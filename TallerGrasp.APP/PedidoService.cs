using System;
using System.Collections.Generic;
using System.Linq;
using TallerGrasp.ENT;
using TallerGrasp.INFRA;

namespace TallerGrasp.APP
{
    public class PedidoService
    {
        private readonly IPedidoRepository _repository;

        public PedidoService(IPedidoRepository repository)
        {
            _repository = repository;
        }

        public Pedido RegistrarPedido(string estudiante, string libro)
        {
            if (string.IsNullOrWhiteSpace(estudiante))
                throw new ArgumentException("El Estudiante es obligatorio.", nameof(estudiante));
            if (string.IsNullOrWhiteSpace(libro))
                throw new ArgumentException("El Libro es obligatorio.", nameof(libro));

            var id = _repository.GetMaxId() + 1;
            var pedido = new Pedido
            {
                Id = id,
                Estudiante = estudiante.Trim(),
                Libro = libro.Trim(),
                Fecha = DateTime.Now
            };

            _repository.Save(pedido);
            return pedido;
        }

        public List<Pedido> ListarPedidos()
        {
            return _repository.GetAll().OrderBy(p => p.Id).ToList();
        }
    }
}

