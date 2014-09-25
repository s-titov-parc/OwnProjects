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

        private static BillItem coffee = new BillItem() { Name = "Coffee", Price = 75 };
        private static BillItem croissant = new BillItem() { Name = "croissant", Price = 100 };
        private static BillItem liquor = new BillItem() { Name = "liquor", Price = 125 };

        [TestMethod]
        public void TestProcess1()
        {
            Bill bill = new Bill();
            bill.Participants.Add(ann);
            bill.Participants.Add(bob);
            bill.Participants.Add(cindy);

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

            Assert.AreEqual(0, bill.Transfers.Sum(t => t.Sum),"Wrong transfers");
        }

        [TestMethod]
        public void TestProcess2()
        {
            Bill bill = new Bill();
            bill.Participants.Add(ann);
            bill.Participants.Add(bob);
            bill.Participants.Add(cindy);

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

            Assert.AreEqual(50, bill.Transfers.Sum(t => t.Sum), "Wrong transfers sum");
            Assert.AreEqual(1, bill.Transfers.Count(t => t.Sum > 0), "Wrong transfers count");
            Assert.AreEqual(ann, bill.Transfers.Single(t => t.Sum > 0).From, "Wrong from");
            Assert.AreEqual(cindy, bill.Transfers.Single(t => t.Sum > 0).To, "Wrong to");
        }
    }
}
