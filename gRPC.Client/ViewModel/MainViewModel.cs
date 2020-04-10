using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Google.Protobuf;
using gRPC.Client.Repository;
using Grpc.Core;
using Grpc.Net.Client;
using HandyControl.Controls;
using PropertyChanged;

namespace gRPC.Client.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        private const string serverAdderss = "https://localhost:5001";  //服务端的地址

        #region dp
        public bool IsChanged { get; set; }
        public string Request1 { get; set; } = string.Empty;
        public string Request2 { get; set; } = string.Empty;
        public string Request3 { get; set; } = "photo.jpg";
        public string Request4 { get; set; } = "老王";
        public string Request5 { get; set; } = "老张,小李";
        public string Response1 { get; set; }
        public string Response2 { get; set; }
        public string Response3 { get; set; }
        public string Response4 { get; set; }
        public string Response5 { get; set; }

        #endregion

        #region Cmd
        public ICommand GetEmployeeByIdCmd => new CommandBase(GetEmployeeById);
        public ICommand GetEmployeeCollectionCmd => new CommandBase(GetEmployeeCollection);
        public ICommand AddPhotoCmd => new CommandBase(AddPhoto);
        public ICommand SaveEmployeeCmd => new CommandBase(SaveEmployee);
        public ICommand SaveEmployeesCmd => new CommandBase(SaveEmployees);
        #endregion

        protected void GetEmployeeById()
        {
            Response1 = string.Empty;
            var metaData = new Metadata     //元数据都是一些键值对字符串
            {
                { "myKey","myValue"}
            };
            if (int.TryParse(Request1, out var id))
            {
                using var channel = GrpcChannel.ForAddress(serverAdderss);
                var client = new EmployeeService.EmployeeServiceClient(channel);
                var response = client.GetEmployeeById(new GetEmployeeByIdRequest
                {
                    Id = id
                });
                Response1 = response.ToString();
                return;
            }

            MessageBox.Show("request is unValid");
        }

        protected async void GetEmployeeCollection()
        {
            Response2 = string.Empty;
            using var channel = GrpcChannel.ForAddress(serverAdderss);
            var client = new EmployeeService.EmployeeServiceClient(channel);

            //发送请求
            using var serverStreamingCall = client.GetEmployeeCollection(new GetEmployeeCollectionRequest
            { IsValid = true, SearchTerm = Request2.Trim() });
            var responseStream = serverStreamingCall.ResponseStream;
            //读取流数据
            while (await responseStream.MoveNext(new CancellationToken()))
            {
                Response2 += responseStream.Current.Employee + Environment.NewLine;
            }
        }

        protected async void AddPhoto()
        {
            Response3 = string.Empty;
            using var channel = GrpcChannel.ForAddress(serverAdderss);
            var client = new EmployeeService.EmployeeServiceClient(channel);

            await using var fs = File.OpenRead(Request3);

            using var clientStreamingCall = client.AddPhoto();
            var requestStream = clientStreamingCall.RequestStream;

            //向“请求流”中写数据
            while (true)
            {
                byte[] buffer = new byte[1024]; //模拟多次传递，将缓存设置小一点
                var length = await fs.ReadAsync(buffer, 0, buffer.Length); //将数据读取到buffer中
                if (length == 0)  //读取完毕
                {
                    break;  //跳出循环
                }
                else if (length < buffer.Length)    //最后一次读取长度无法填满buffer的长度
                {
                    Array.Resize(ref buffer, length);   //改变buffer数组的长度
                }
                var streamData = ByteString.CopyFrom(buffer);   //将byte数组数据转成传递时需要的ByteString类型
                await requestStream.WriteAsync(new AddPhotoRequest { Photo = streamData });
            }

            await requestStream.CompleteAsync();  //告知服务端数据传递完毕
            var response = await clientStreamingCall.ResponseAsync;
            Response3 = response.IsOK ? "congratulations" : "ah oh";
        }

        protected async void SaveEmployee()
        {
            Response4 = string.Empty;
            using var channel = GrpcChannel.ForAddress(serverAdderss);
            var client = new EmployeeService.EmployeeServiceClient(channel);
            var newEmp = GetNewEmployee(Request4);
            var response = client.SaveEmployee(new EmployeeRequest { Employee = newEmp });
            Response5 += $"New Employee “{response.Employee.Name}” is Saved";
        }


        protected async void SaveEmployees()
        {
            Response5 = string.Empty;
            using var channel = GrpcChannel.ForAddress(serverAdderss);
            var client = new EmployeeService.EmployeeServiceClient(channel);

            var serverStreamingCall = client.SaveEmployees();
            //因为是双向流的方式，我们需要同时操作“请求流”和“响应流”
            var requestStream = serverStreamingCall.RequestStream;
            var responseStream = serverStreamingCall.ResponseStream;
            //获取员工数据
            var employees = GetNewEmployees(Request5.Trim());

            //依次将员工数据写入请求流中
            foreach (var employee in employees)
            {
                await requestStream.WriteAsync(new EmployeeRequest { Employee = employee });
            }
            //告知服务端数据传递完毕
            await requestStream.CompleteAsync();
            //读取服务端返回的流式数据
            await Task.Run(async () =>
            {
                while (await responseStream.MoveNext(new CancellationToken()))
                {
                    Response5 += $"New Employee “{responseStream.Current.Employee.Name}” is Saved"
                        + Environment.NewLine;
                }
            });
        }

        private Employee GetNewEmployee(string employeeName)
        {
            var employee = ClientRepository.NewEmployees.Find(emp => emp.Name == employeeName);
            if (employee != null) return employee;
            MessageBox.Show("没有这个人");
            return null;
        }

        private List<Employee> GetNewEmployees(string employeeNames)
        {
            var names = employeeNames.Split(",");
            var empList = names.Select(empName =>
                ClientRepository.NewEmployees.Find(emp => emp.Name == empName))
                .Where(employee => employee != null).ToList();
            if (empList.Count != 0) return empList;
            MessageBox.Show("没有这些人");
            return null;
        }
    }
}
