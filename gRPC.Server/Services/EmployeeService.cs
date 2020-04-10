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
        public override async Task<EmployeeResponse> GetEmployeeById(GetEmployeeByIdRequest request, ServerCallContext context)
        {
            //读取请求头中的元数据
            var metaData = context.RequestHeaders;
            foreach (var data in metaData)
            {
                Console.WriteLine($"{data.Key} => {data.Value}");
            }

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
            var employees = new List<Employee>();
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
                await responseStream.WriteAsync(
                    new GetEmployeeCollectionReponse { Employee = employee });
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
                Console.WriteLine($"{++count} : receive requestStreamData's length is {requestStream.Current.Photo.Length}");
            }
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
                var newEmployee = requestStream.Current.Employee;
                if (!EmployeeRepository.Emloyees.Exists(emp => emp.Id == newEmployee.Id))
                {
                    EmployeeRepository.Emloyees.Add(newEmployee);
                }
                Console.WriteLine($"receive NewEmployee {newEmployee.Name}");
                await responseStream.WriteAsync(new EmployeeResponse()
                {
                    Employee = newEmployee
                });
            }
        }
    }
}
