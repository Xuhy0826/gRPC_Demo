using System.Windows.Input;
using Grpc.Net.Client;
using HandyControl.Controls;
using PropertyChanged;

namespace gRPC.Client.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public bool IsChanged { get; set; }
        public string Request1 { get; set; }
        public string Request2 { get; set; }
        public string Request3 { get; set; }
        public string Request4 { get; set; }
        public string Request5 { get; set; }
        public string Response1 { get; set; }
        public string Response2 { get; set; }
        public string Response3 { get; set; }
        public string Response4 { get; set; }
        public string Response5 { get; set; }

        public ICommand GetEmployeeByIdCmd => new CommandBase(GetEmployeeById);

        protected void GetEmployeeById()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new EmployeeService.EmployeeServiceClient(channel);
            if (int.TryParse(Request1, out var id))
            {
                var response = client.GetEmployeeById(new GetEmployeeByIdRequest
                {
                    Id = id
                });
                Response1 = response.ToString();
                return;
            }
            MessageBox.Show("request is unValid");
        }
    }
}
