using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Atividade
{
    class Program
    {
        private const int LarguraCupom = 32;

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            var clientes = new List<Cliente>();
            var produtos = new List<Produto>();
            var vendas = new List<Venda>();
            var proximoClienteId = 1;
            var proximoProdutoId = 1;
            var proximaVendaId = 1;

            while (true)
            {
                Console.WriteLine("\n=== DOCE DA SOL - SISTEMA DE VENDAS ===");
                Console.WriteLine("1 - Cadastrar cliente");
                Console.WriteLine("2 - Cadastrar produto");
                Console.WriteLine("3 - Registrar venda do dia");
                Console.WriteLine("4 - Imprimir cupom não fiscal");
                Console.WriteLine("5 - Relatório mensal por cliente");
                Console.WriteLine("0 - Sair");
                Console.Write("Escolha uma opção: ");

                var opcao = Console.ReadLine();
                switch (opcao)
                {
                    case "1":
                        clientes.Add(CadastrarCliente(proximoClienteId++));
                        break;
                    case "2":
                        produtos.Add(CadastrarProduto(proximoProdutoId++));
                        break;
                    case "3":
                        if (!VerificarLista(clientes, "clientes"))
                        {
                            break;
                        }
                        if (!VerificarLista(produtos, "produtos"))
                        {
                            break;
                        }
                        var venda = RegistrarVenda(clientes, produtos, proximaVendaId++);
                        vendas.Add(venda);
                        Console.WriteLine("Venda registrada com sucesso.");
                        break;
                    case "4":
                        if (!VerificarLista(vendas, "vendas"))
                        {
                            break;
                        }
                        var vendaCupom = SelecionarVenda(vendas);
                        if (vendaCupom != null)
                        {
                            Console.WriteLine(GerarCupomNaoFiscal(vendaCupom));
                        }
                        break;
                    case "5":
                        if (!VerificarLista(clientes, "clientes"))
                        {
                            break;
                        }
                        if (!VerificarLista(vendas, "vendas"))
                        {
                            break;
                        }
                        GerarRelatorioMensal(clientes, vendas);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        private static Cliente CadastrarCliente(int id)
        {
            Console.WriteLine("\n--- Cadastro de Cliente ---");
            var nome = LerTextoObrigatorio("Nome do cliente");
            var endereco = LerTextoObrigatorio("Endereço");
            return new Cliente
            {
                Id = id,
                Nome = nome,
                Endereco = endereco
            };
        }

        private static Produto CadastrarProduto(int id)
        {
            Console.WriteLine("\n--- Cadastro de Produto ---");
            var nome = LerTextoObrigatorio("Nome do produto");
            var preco = LerDecimal("Preço do produto");
            return new Produto
            {
                Id = id,
                Nome = nome,
                Preco = preco
            };
        }

        private static Venda RegistrarVenda(List<Cliente> clientes, List<Produto> produtos, int id)
        {
            Console.WriteLine("\n--- Registrar Venda ---");
            var cliente = SelecionarCliente(clientes);
            var dataVenda = LerData("Data da venda (dd/mm/aaaa) ou vazio para hoje", DateTime.Today);
            var itens = new List<ItemVenda>();

            while (true)
            {
                var produto = SelecionarProduto(produtos);
                var quantidade = LerInteiro("Quantidade");
                itens.Add(new ItemVenda
                {
                    Produto = produto,
                    Quantidade = quantidade
                });

                Console.Write("Adicionar outro produto? (s/n): ");
                var resposta = Console.ReadLine();
                if (!string.Equals(resposta, "s", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            var dataPagamento = LerDataOpcional("Data de pagamento (dd/mm/aaaa) ou vazio para preencher no cupom");

            return new Venda
            {
                Id = id,
                Cliente = cliente,
                DataVenda = dataVenda,
                Itens = itens,
                DataPagamento = dataPagamento
            };
        }

        private static Cliente SelecionarCliente(List<Cliente> clientes)
        {
            Console.WriteLine("\nClientes cadastrados:");
            foreach (var cliente in clientes)
            {
                Console.WriteLine($"{cliente.Id} - {cliente.Nome} ({cliente.Endereco})");
            }
            var id = LerInteiro("Informe o ID do cliente");
            return clientes.First(c => c.Id == id);
        }

        private static Produto SelecionarProduto(List<Produto> produtos)
        {
            Console.WriteLine("\nProdutos cadastrados:");
            foreach (var produto in produtos)
            {
                Console.WriteLine($"{produto.Id} - {produto.Nome} - {produto.Preco:C}");
            }
            var id = LerInteiro("Informe o ID do produto");
            return produtos.First(p => p.Id == id);
        }

        private static Venda? SelecionarVenda(List<Venda> vendas)
        {
            Console.WriteLine("\nVendas registradas:");
            foreach (var venda in vendas)
            {
                Console.WriteLine($"{venda.Id} - {venda.Cliente.Nome} - {venda.DataVenda:dd/MM/yyyy} - {venda.Total:C}");
            }
            var id = LerInteiro("Informe o ID da venda");
            return vendas.FirstOrDefault(v => v.Id == id);
        }

        private static string GerarCupomNaoFiscal(Venda venda)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Centralizar("DOCE DA SOL"));
            sb.AppendLine(Centralizar("CUPOM NÃO FISCAL"));
            sb.AppendLine(new string('-', LarguraCupom));
            sb.AppendLine($"Data: {venda.DataVenda:dd/MM/yyyy}");
            sb.AppendLine($"Cliente: {venda.Cliente.Nome}");
            sb.AppendLine($"Endereço: {venda.Cliente.Endereco}");
            sb.AppendLine(new string('-', LarguraCupom));
            sb.AppendLine("Itens:");

            foreach (var item in venda.Itens)
            {
                sb.AppendLine(TruncarTexto(item.Produto.Nome, LarguraCupom));
                var linha = $"{item.Quantidade}x {item.Produto.Preco:C} = {item.Total:C}";
                sb.AppendLine(TruncarTexto(linha, LarguraCupom));
            }

            sb.AppendLine(new string('-', LarguraCupom));
            sb.AppendLine($"TOTAL: {venda.Total:C}");
            sb.AppendLine(new string('-', LarguraCupom));
            sb.AppendLine("Assinatura: __________________");
            sb.AppendLine(FormatarDataPagamento(venda.DataPagamento));
            sb.AppendLine(new string('-', LarguraCupom));
            sb.AppendLine(Centralizar("OBRIGADO PELA PREFERÊNCIA"));
            return sb.ToString();
        }

        private static void GerarRelatorioMensal(List<Cliente> clientes, List<Venda> vendas)
        {
            Console.WriteLine("\n--- Relatório Mensal ---");
            var cliente = SelecionarCliente(clientes);
            var mes = LerInteiro("Informe o mês (1-12)");
            var ano = LerInteiro("Informe o ano (ex: 2024)");

            var vendasCliente = vendas
                .Where(v => v.Cliente.Id == cliente.Id && v.DataVenda.Month == mes && v.DataVenda.Year == ano)
                .OrderBy(v => v.DataVenda)
                .ToList();

            if (vendasCliente.Count == 0)
            {
                Console.WriteLine("Não há vendas para este cliente no período informado.");
                return;
            }

            Console.WriteLine($"\nRelatório de {cliente.Nome} - {mes:D2}/{ano}");
            Console.WriteLine(new string('-', 40));

            foreach (var venda in vendasCliente)
            {
                Console.WriteLine($"Data: {venda.DataVenda:dd/MM/yyyy} - Total: {venda.Total:C}");
                foreach (var item in venda.Itens)
                {
                    Console.WriteLine($"  {item.Quantidade}x {item.Produto.Nome} ({item.Produto.Preco:C}) = {item.Total:C}");
                }
            }

            var totalMes = vendasCliente.Sum(v => v.Total);
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"Total do mês: {totalMes:C}");
        }

        private static string LerTextoObrigatorio(string rotulo)
        {
            while (true)
            {
                Console.Write($"{rotulo}: ");
                var entrada = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(entrada))
                {
                    return entrada.Trim();
                }
                Console.WriteLine("Valor obrigatório.");
            }
        }

        private static decimal LerDecimal(string rotulo)
        {
            while (true)
            {
                Console.Write($"{rotulo}: ");
                var entrada = Console.ReadLine();
                if (decimal.TryParse(entrada, out var valor) && valor >= 0)
                {
                    return valor;
                }
                Console.WriteLine("Informe um valor válido.");
            }
        }

        private static int LerInteiro(string rotulo)
        {
            while (true)
            {
                Console.Write($"{rotulo}: ");
                var entrada = Console.ReadLine();
                if (int.TryParse(entrada, out var valor) && valor > 0)
                {
                    return valor;
                }
                Console.WriteLine("Informe um número válido.");
            }
        }

        private static DateTime LerData(string rotulo, DateTime valorPadrao)
        {
            while (true)
            {
                Console.Write($"{rotulo}: ");
                var entrada = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(entrada))
                {
                    return valorPadrao;
                }
                if (DateTime.TryParse(entrada, out var data))
                {
                    return data.Date;
                }
                Console.WriteLine("Informe uma data válida.");
            }
        }

        private static DateTime? LerDataOpcional(string rotulo)
        {
            while (true)
            {
                Console.Write($"{rotulo}: ");
                var entrada = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(entrada))
                {
                    return null;
                }
                if (DateTime.TryParse(entrada, out var data))
                {
                    return data.Date;
                }
                Console.WriteLine("Informe uma data válida.");
            }
        }

        private static bool VerificarLista<T>(List<T> lista, string nome)
        {
            if (lista.Count == 0)
            {
                Console.WriteLine($"Nenhum {nome} cadastrado.");
                return false;
            }
            return true;
        }

        private static string Centralizar(string texto)
        {
            if (texto.Length >= LarguraCupom)
            {
                return texto;
            }
            var espacos = (LarguraCupom - texto.Length) / 2;
            return new string(' ', espacos) + texto;
        }

        private static string TruncarTexto(string texto, int largura)
        {
            if (texto.Length <= largura)
            {
                return texto;
            }
            return texto.Substring(0, largura);
        }

        private static string FormatarDataPagamento(DateTime? dataPagamento)
        {
            if (dataPagamento.HasValue)
            {
                return $"Data de pagamento: {dataPagamento:dd/MM/yyyy}";
            }
            return "Data de pagamento: ___/___/____";
        }
    }

    class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
    }

    class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }

    class ItemVenda
    {
        public Produto Produto { get; set; } = new Produto();
        public int Quantidade { get; set; }
        public decimal Total => Quantidade * Produto.Preco;
    }

    class Venda
    {
        public int Id { get; set; }
        public Cliente Cliente { get; set; } = new Cliente();
        public DateTime DataVenda { get; set; }
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
        public DateTime? DataPagamento { get; set; }
        public decimal Total => Itens.Sum(i => i.Total);
    }
}
