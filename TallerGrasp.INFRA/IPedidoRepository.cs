using System.Collections.Generic;
using TallerGrasp.ENT;

namespace TallerGrasp.INFRA
{
    public interface IPedidoRepository
    {
        IEnumerable<Pedido> GetAll();
        void Save(Pedido pedido);
        int GetMaxId();
    }
}

