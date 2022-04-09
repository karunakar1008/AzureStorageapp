using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace TableStorageApp
{
    class Employee : TableEntity
    {
        public string Email { get; set; }
        public int PhoneNumber { get; set; }
        public Employee() { }
        public Employee(string deptName, string empName)
        {
            this.PartitionKey = deptName;
            this.RowKey = empName;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfiguration config = builder.Build();
            string cs = config["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cs);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableEmployee = tableClient.GetTableReference("employee");
            tableEmployee.CreateIfNotExists();

            Employee emp1 = new Employee("Training", "Emp1") { Email = "emp1@test.com", PhoneNumber = 11111 };
            Employee emp2 = new Employee("Training", "Emp2") { Email = "emp2@test.com", PhoneNumber = 22222 };
            Employee emp3 = new Employee("Development", "Emp1")
            {
                Email = "emp3@test.com",
                PhoneNumber = 33333
            };

            TableBatchOperation batchOperation = new TableBatchOperation();
            batchOperation.InsertOrReplace(emp1);
            batchOperation.InsertOrReplace(emp2);
            tableEmployee.ExecuteBatch(batchOperation);

            batchOperation = new TableBatchOperation();
            batchOperation.InsertOrReplace(emp3);
            tableEmployee.ExecuteBatch(batchOperation);

            TableOperation operation = TableOperation.Retrieve<Employee>("Development", "Emp1");
            TableResult result = tableEmployee.Execute(operation);
            emp3 = result.Result as Employee;

            //emp3.ETag = "*";
            //batchOperation = new TableBatchOperation();
            //batchOperation.Delete(emp3);
            //tableEmployee.ExecuteBatch(batchOperation);

            TableQuery<Employee> query = new TableQuery<Employee>();
            string filter = TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, "emp1@test.com");
            query = query.Where(filter);
            var emps = tableEmployee.ExecuteQuery(query);
            foreach (Employee emp in emps)
            {
                Console.WriteLine(emp.PartitionKey + " " + emp.RowKey + " " + emp.Email);
            }
            //tableEmployee.DeleteIfExists();
        }
    }
}
