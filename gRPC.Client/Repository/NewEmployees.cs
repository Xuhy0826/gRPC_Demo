using System.Collections.Generic;

namespace gRPC.Client.Repository
{
    public class ClientRepository
    {
        public static List<Employee> NewEmployees => new List<Employee>
        {
            new Employee
            {
                Id = 5,
                Name = "老王",
                Gender = Gender.Male,
                EmployeeNo = 500,
                Department = "D5",
                BirthDay = new Date{Day = 31, Month = 2, Year = 1975},
                IsValid = true
            },
            new Employee
            {
                Id = 6,
                Name = "老张",
                Gender = Gender.Male,
                EmployeeNo = 502,
                Department = "D6",
                BirthDay = new Date{Day = 23, Month = 2, Year = 1980},
                IsValid = true
            },
            new Employee
            {
                Id = 7,
                Name = "小李",
                Gender = Gender.Female,
                EmployeeNo = 504,
                Department = "D7",
                BirthDay = new Date{Day = 29, Month = 2, Year = 1992},
                IsValid = true
            }
        };
    }
}
