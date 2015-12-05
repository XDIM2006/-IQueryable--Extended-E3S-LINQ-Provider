using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample03.E3SClient.Entities;
using Sample03.E3SClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Sample03
{
	[TestClass]
	public class E3SProviderTests
	{
		[TestMethod]
		public void WithoutProvider()
		{
			var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"] , ConfigurationManager.AppSettings["password"]);
            
            var res = client.SearchFTS<EmployeeEntity>(new List<Statement>() {
                    new Statement {
                        Query = "workstation:(EPRUIZHW0060)"
                    },
                     new Statement {
                        Query = "workstation:(EPRUIZ*)"
                    }
                }, 0, 1);

			foreach (var emp in res)
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
			}
		}

		[TestMethod]
		public void WithoutProviderNonGeneric()
		{
			var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
			var res = client.SearchFTS(typeof(EmployeeEntity), new List<Statement>() {
                    new Statement {
                        Query = "workstation:(EPRUIZHW0060)"
                    }
                }, 0, 10);

			foreach (var emp in res.OfType<EmployeeEntity>())
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
			}
		}


		[TestMethod]
		public void WithProvider()
		{
			var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            /*
            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0060"))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
            foreach (var emp in employees.Where(e => "EPRUIZHW0060" == e.workstation))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
            */
            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0060" 
            && e.workstation.Contains("IZHW006")
            && e.workstation.EndsWith("IZHW0060")
            && e.workstation.StartsWith("EPRUIZHW006")))
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
			}
		}
	}
}
