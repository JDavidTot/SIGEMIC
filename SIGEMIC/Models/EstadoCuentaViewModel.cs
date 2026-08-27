namespace SIGEMIC.Models
{
    public class EstadoCuentaViewModel
    {
        public string CodigoCliente { get; set; } = "";
        public string CodigoCredito { get; set; } = "";
        public decimal MontoOriginal { get; set; }
        public decimal SaldoCapital { get; set; }
        public decimal CuotaOriginal { get; set; }
        public string EstadoCredito { get; set; } = "";

        public decimal TotalPagado { get; set; }
        public decimal TotalCapitalPagado { get; set; }
        public decimal TotalInteresPagado { get; set; }
        public decimal TotalAbonosExtraordinarios { get; set; }

        public List<PagoHistorialViewModel> Pagos { get; set; }
            = new List<PagoHistorialViewModel>();
    }

    public class PagoHistorialViewModel
    {
        public ulong IdPago { get; set; }
        public string NumeroRecibo { get; set; } = "";
        public DateTime FechaPago { get; set; }

        public string TipoOperacion { get; set; } = "";

        public uint? NumeroCuota { get; set; }

        public string FormaPago { get; set; } = "";

        public decimal MontoTotal { get; set; }
        public decimal CapitalAplicado { get; set; }
        public decimal InteresAplicado { get; set; }
        public decimal MoraAplicada { get; set; }

        public string? Observacion { get; set; }
    }
}