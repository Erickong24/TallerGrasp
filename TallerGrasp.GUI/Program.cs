using System;
using System.IO;
using TallerGrasp.APP;
using TallerGrasp.INFRA;

namespace TallerGrasp.GUI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pedidos.txt");
            var repository = new PedidoRepositoryTxt(filePath);
            var service = new PedidoService(repository);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Biblioteca - Gestión de Pedidos");
                Console.WriteLine("1) Registrar un nuevo pedido");
                Console.WriteLine("2) Listar todos los pedidos");
                Console.WriteLine("0) Salir");
                Console.Write("Seleccione una opción: ");
                var opt = Console.ReadLine();

                switch (opt)
                {
                    case "1":
                        Registrar(service);
                        break;
                    case "2":
                        Listar(service);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opción inválida.");
                        Pause();
                        break;
                }
            }
        }

        private static void Registrar(PedidoService service)
        {
            Console.Clear();
            Console.WriteLine("Registrar Pedido");
            Console.Write("Estudiante: ");
            string estudiante = Console.ReadLine() ?? string.Empty;
            Console.Write("Libro: ");
            string libro = Console.ReadLine() ?? string.Empty;

            try
            {
                var pedido = service.RegistrarPedido(estudiante, libro);
                Console.WriteLine("Pedido registrado:");
                Console.WriteLine($"{pedido.Id} | {pedido.Estudiante} | {pedido.Libro} | {pedido.Fecha:yyyy-MM-dd HH:mm:ss}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }

            Pause();
        }

        private static void Listar(PedidoService service)
        {
            Console.Clear();
            Console.WriteLine("Listado de Pedidos");
            var pedidos = service.ListarPedidos();

            if (pedidos.Count == 0)
            {
                Console.WriteLine("(No hay pedidos aún)");
            }
            else
            {
                foreach (var p in pedidos)
                {
                    Console.WriteLine($"{p.Id} | {p.Estudiante} | {p.Libro} | {p.Fecha:yyyy-MM-dd HH:mm:ss}");
                }
            }

            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("Presione una tecla para continuar...");
            Console.ReadKey();
        }
    }
}

