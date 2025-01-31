namespace AI_Chatbot.DTOs
{
    public class InsuranceDto
    {
        public string? InsuranceName { get; set; }
        public string? InsuranceStart { get; set; } //Format dd/mm/yy
        public string? InsuranceEnd { get; set; }  //Format dd/mm/yy
        public string? InsuranceStatus { get; set; } //active or deactive
    }
}
