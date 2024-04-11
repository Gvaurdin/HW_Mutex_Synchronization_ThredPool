using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp_The_Casino_app
{
    internal class Player
    {
        private readonly int startingMoney;
        private int money;
        private Random random;

        public Player(int startingMoney)
        {
            this.startingMoney = startingMoney;
            this.money = startingMoney;
            random = new Random();
        }

        public void Play()
        {
            while (true)
            {
                int betAmount = random.Next(50, 200); // Random bet amount
                int betNumber = random.Next(0, 36); // Random bet number

                int result = CasinoMainWindow.Instance.SpinRoulette();

                if (result == betNumber)
                {
                    money += betAmount;
                    CasinoMainWindow.Instance.UpdateUI($"Player {Thread.CurrentThread.ManagedThreadId}: Wins {betAmount}, Total Money: {money}");
                }
                else
                {
                    money -= betAmount;
                    CasinoMainWindow.Instance.UpdateUI($"Player {Thread.CurrentThread.ManagedThreadId}: Loses {betAmount}, Total Money: {money}");
                }

                if (money <= 0)
                {
                    CasinoMainWindow.Instance.UpdateUI($"Player {Thread.CurrentThread.ManagedThreadId}: Out of money, leaving the table.");
                    break;
                }

                Thread.Sleep(5000); // Simulate time between spins
            }
        }
    }
}
