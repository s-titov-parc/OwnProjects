using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BillSplitter.Models;
using System.Linq;

namespace BillSplitter.Tests
{
    [TestClass]
    public class TestBill
    {
        private static Participant ann = new Participant() { FullName = "Ann", Id = "Ann" };
        private static Participant bob = new Participant() { FullName = "Bob", Id = "Bob" };
        private static Participant cindy = new Participant() { FullName = "Cindy", Id = "Cindy" };

        private static BillItem get_coffee()
		{
			return new BillItem() { Name = "Coffee", Price = 75 };
		}
		private static BillItem get_croissant()
		{
			return  new BillItem() { Name = "croissant", Price = 100 };
			}
		private static BillItem get_liquor()
		{
			return  new BillItem() { Name = "liquor", Price = 125 };
			}

        [TestMethod]
        public void TestProcess1()
        {
            Bill bill = new Bill();
            bill.Participants.Add(ann);
            bill.Participants.Add(bob);
            bill.Participants.Add(cindy);

			var coffee = get_coffee();
			var croissant = get_croissant();
			var liquor = get_liquor();

            bill.Items.Add(coffee);
            bill.Items.Add(croissant);
            bill.Items.Add(liquor);

            coffee.Participants.Add(ann);
            coffee.Participants.Add(bob);
            coffee.Participants.Add(cindy);

            croissant.Participants.Add(ann);
            croissant.Participants.Add(bob);
            croissant.Participants.Add(cindy);

            liquor.Participants.Add(ann);
            liquor.Participants.Add(bob);
            liquor.Participants.Add(cindy);

            bill.Payments.Add(new Payment(ann, 100));
            bill.Payments.Add(new Payment(bob, 100));
            bill.Payments.Add(new Payment(cindy, 100));

            bill.Process();
			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "Wrong transfers");
			
			bill.Process();
			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum),"2 - Wrong transfers");
        }

        [TestMethod]
        public void TestProcess2()
        {
            Bill bill = new Bill();
            bill.Participants.Add(ann);
            bill.Participants.Add(bob);
            bill.Participants.Add(cindy);

			var coffee = get_coffee();
			var croissant = get_croissant();
			var liquor = get_liquor();

            bill.Items.Add(coffee);
            bill.Items.Add(croissant);
            bill.Items.Add(liquor);

            coffee.Participants.Add(ann);
            coffee.Participants.Add(bob);
            coffee.Participants.Add(cindy);

            croissant.Participants.Add(ann);
            croissant.Participants.Add(bob);
            croissant.Participants.Add(cindy);

            liquor.Participants.Add(ann);
            liquor.Participants.Add(bob);
            liquor.Participants.Add(cindy);

            bill.Payments.Add(new Payment(ann, 50));
            bill.Payments.Add(new Payment(bob, 100));
            bill.Payments.Add(new Payment(cindy, 150));

            bill.Process();

            Assert.AreEqual(0, Math.Round(bill.Transfers.Sum(t => t.Sum)-50,2), "Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");
            Assert.AreEqual(1, bill.Transfers.Count(t => t.Sum > 0), "Wrong transfers count");
            Assert.AreEqual(ann, bill.Transfers.Single(t => t.Sum > 0).From, "Wrong from");
            Assert.AreEqual(cindy, bill.Transfers.Single(t => t.Sum > 0).To, "Wrong to");

			bill.Process();

			Assert.AreEqual(0, Math.Round(bill.Transfers.Sum(t => t.Sum) - 50, 2), "2 - Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
			Assert.AreEqual(1, bill.Transfers.Count(t => t.Sum > 0), "2 - Wrong transfers count");
			Assert.AreEqual(ann, bill.Transfers.Single(t => t.Sum > 0).From, "2 - Wrong from");
			Assert.AreEqual(cindy, bill.Transfers.Single(t => t.Sum > 0).To, "2 - Wrong to");
        }

		[TestMethod]
		public void TestProcessChange()
		{
			Bill bill = new Bill();
			bill.Participants.Add(ann);
			bill.Participants.Add(bob);
			bill.Participants.Add(cindy);

			var coffee = get_coffee();
			var croissant = get_croissant();
			var liquor = get_liquor();

			bill.Items.Add(coffee);
			bill.Items.Add(croissant);
			bill.Items.Add(liquor);

			coffee.Participants.Add(ann);
			coffee.Participants.Add(bob);
			coffee.Participants.Add(cindy);

			croissant.Participants.Add(ann);
			croissant.Participants.Add(bob);
			croissant.Participants.Add(cindy);

			liquor.Participants.Add(ann);
			liquor.Participants.Add(bob);
			liquor.Participants.Add(cindy);

			bill.Payments.Add(new Payment(ann, 100));
			bill.Payments.Add(new Payment(bob, 100));
			bill.Payments.Add(new Payment(cindy, 200));

			bill.Process();

			Assert.AreEqual(0, Math.Round(bill.Transfers.Sum(t => t.Sum),2), "Wrong transfers sum");
			Assert.AreEqual(1, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");
			Assert.AreEqual(cindy, bill.Changes.Single(t => t.Sum > 0).Participant, "Wrong to");

			bill.Process();

			Assert.AreEqual(0, Math.Round(bill.Transfers.Sum(t => t.Sum), 2), "2 - Wrong transfers sum");
			Assert.AreEqual(1, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
			Assert.AreEqual(cindy, bill.Changes.Single(t => t.Sum > 0).Participant, "2 - Wrong to");
		}

		[TestMethod]
		public void TestProcessOneParticipant()
		{
			Bill bill = new Bill();
			bill.Participants.Add(ann);

			bill.Payments.Add(new Payment(ann, 50));

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "2 - Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
		}

		[TestMethod]
		public void TestProcessTwoParticipants()
		{
			Bill bill = new Bill();
			bill.Participants.Add(ann);
			bill.Participants.Add(bob);

			bill.Payments.Add(new Payment(ann, 50));
			bill.Payments.Add(new Payment(bob, 100));

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "Wrong transfers sum");
			Assert.AreEqual(2, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");
			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "2 - Wrong transfers sum");
			Assert.AreEqual(2, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
		}

		[TestMethod]
		public void TestProcessEmpty()
		{
			Bill bill = new Bill();

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "2 - Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
		}

		[TestMethod]
		public void TestProcessNoParticipants()
		{
			Bill bill = new Bill();

			var coffee = get_coffee();
			var croissant = get_croissant();
			var liquor = get_liquor();

			bill.Items.Add(coffee);
			bill.Items.Add(croissant);
			bill.Items.Add(liquor);

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "Wrong changes count");

			bill.Process();

			Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum), "2 - Wrong transfers sum");
			Assert.AreEqual(0, bill.Changes.Count(t => t.Sum > 0), "2 - Wrong changes count");
		}
    }
}
