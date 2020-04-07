using System.Collections.Generic;

namespace gRPC.Server.Repository
{
    public class EmployeeRepository
    {
        public static List<Employee> Emloyees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                Name = "John",
                Gender = Gender.Male,
                EmployeeNo = 100,
                Department = "D1",
                BirthDay = new Date{Day = 26, Month = 8, Year = 1992},
                IsValid = true
            },
            new Employee
            {
                Id = 2,
                Name = "Jack",
                Gender = Gender.Male,
                EmployeeNo = 102,
                Department = "D1",
                BirthDay = new Date{Day = 23, Month = 9, Year = 1989},
                IsValid = true
            },
            new Employee
            {
                Id = 3,
                Name = "Alice",
                Gender = Gender.Female,
                EmployeeNo = 104,
                Department = "D2",
                BirthDay = new Date{Day = 12, Month = 5, Year = 1990},
                IsValid = true
            },
            new Employee
            {
                Id = 4,
                Name = "Rose",
                Gender = Gender.Female,
                EmployeeNo = 106,
                Department = "D2",
                BirthDay = new Date{Day = 5, Month = 12, Year = 1996},
                IsValid = true
            },
        };
    }
}
