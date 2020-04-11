using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using gRPC.Server.Repository;

namespace gRPC.Server.Services
{
    public class GrpcEmployeeService : EmployeeService.EmployeeServiceBase
    {
        /// <summary>
        /// 一元操作演示 —— 根据id获取员工数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<EmployeeResponse> GetEmployeeById(GetEmployeeByIdRequest request,
            ServerCallContext context)
        {
            //读取请求头中的元数据(应用层自定义的 key-value 对)
            var metaDataIdHeaders = context.RequestHeaders;
            foreach (var data in metaDataIdHeaders)
            {
                Console.WriteLine($"{data.Key} => {data.Value}");
            }

            //根据请求的Id找到员工信息
            var employee = EmployeeRepository.Emloyees.SingleOrDefault(emp => emp.Id == request.Id);

            if (employee == null)
                throw new RpcException(Status.DefaultSuccess
                    , $"Employee of {request.Id} is not found");

            var response = new EmployeeResponse { Employee = employee };
            return await Task.FromResult(response);
        }

        /// <summary>
        /// 服务端流演示 —— 根据条件获取员工数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetEmployeeCollection(GetEmployeeCollectionRequest request, IServerStreamWriter<GetEmployeeCollectionReponse> responseStream,
            ServerCallContext context)
        {
            List<Employee> employees;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))  //有条件就根据条件查询
            {
                employees = EmployeeRepository.Emloyees
                      .FindAll(emp => emp.Name.Contains(request.SearchTerm) ||
                                      emp.Department.Contains(request.SearchTerm) ||
                                      emp.EmployeeNo.ToString().Contains(request.SearchTerm));
            }
            else
            {
                employees = EmployeeRepository.Emloyees;
            }
            employees = employees.FindAll(emp => emp.IsValid == request.IsValid);

            foreach (var employee in employees)
            {
                //***************************向响应流中写入数据*************************************
                await responseStream.WriteAsync(new GetEmployeeCollectionReponse { Employee = employee });
                //******************************************************************************
            }
        }

        /// <summary>
        /// 员工照片上传
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<AddPhotoReponse> AddPhoto(IAsyncStreamReader<AddPhotoRequest> requestStream, ServerCallContext context)
        {
            var buffer = new List<byte>();
            var count = 0;
            while (await requestStream.MoveNext(new CancellationToken()))
            {
                buffer.AddRange(requestStream.Current.Photo);
                //每接收一次请求打印一条消息来显示
                Console.WriteLine($"{++count} : receive requestStreamData's length is {requestStream.Current.Photo.Length}");
            }
            //只是将收到的全部数据还原成原来的图片数据
            File.WriteAllBytes(@"photo.jpg", buffer.ToArray());
            return new AddPhotoReponse { IsOK = true };
        }

        public override async Task<EmployeeResponse> SaveEmployee(EmployeeRequest request, ServerCallContext context)
        {
            if (request.Employee == null) return new EmployeeResponse();
            //将新员工数据添加到服务端
            if (!EmployeeRepository.Emloyees.Exists(emp => emp.Id == request.Employee.Id))
            {
                EmployeeRepository.Emloyees.Add(request.Employee);
            }
            Console.WriteLine($"receive NewEmployee {request.Employee.Name}");
            return await Task.FromResult(new EmployeeResponse { Employee = request.Employee });
        }

        public override async Task SaveEmployees(IAsyncStreamReader<EmployeeRequest> requestStream, IServerStreamWriter<EmployeeResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(new CancellationToken()))
            {
                //从请求流中获取数据
                var newEmployee = requestStream.Current.Employee;
                if (!EmployeeRepository.Emloyees.Exists(emp => emp.Id == newEmployee.Id))
                {
                    EmployeeRepository.Emloyees.Add(newEmployee);
                }
                //每存储一条员工数据后在控制台上打印一条记录
                Console.WriteLine($"receive NewEmployee {newEmployee.Name}");
                //每存储一条员工数据后向响应流中写入数据返回给客户端
                await responseStream.WriteAsync(new EmployeeResponse()
                {
                    Employee = newEmployee
                });
            }
        }
    }
}
