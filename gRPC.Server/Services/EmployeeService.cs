using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using gRPC.Server.Repository;

namespace gRPC.Server.Services
{
    public class GrpcEmployeeService : EmployeeService.EmployeeServiceBase
    {
        public override async Task<EmployeeResponse> GetEmployeeById(GetEmployeeByIdRequest request, ServerCallContext context)
        {
            var employee = EmployeeRepository.Emloyees.SingleOrDefault(emp => emp.Id == request.Id);
            if (employee != null)
            {
                var response = new EmployeeResponse { Employee = employee };
                return await Task.FromResult(response);
            }
            throw new RpcException(Status.DefaultSuccess, $"Employee of {request.Id} is not found");
        }
    }
}
