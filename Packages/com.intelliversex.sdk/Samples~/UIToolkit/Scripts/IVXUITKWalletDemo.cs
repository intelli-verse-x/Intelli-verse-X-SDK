using System;
using System.Threading.Tasks;
using IntelliVerseX.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Wallet Demo")]
    public sealed class IVXUITKWalletDemo : IVXUITKFeatureDemoBase
    {
        private TextField _amountField;

        protected override void BuildFeatureUI()
        {
            SetTitle("Wallet", "Game + global balances via IVXNWalletManager");
            _amountField = AddField("amount", "Amount", "100");
            AddAction("Refresh", () => _ = RefreshAsync());
            AddAction("Credit Game", () => _ = CreditGameAsync());
            AddAction("Spend Game", () => _ = SpendGameAsync());
            AddAction("Credit Global", () => _ = CreditGlobalAsync());
            AddAction("Spend Global", () => _ = SpendGlobalAsync());
            IVXNWalletManager.Initialize();
            UpdateBalances();
            SetStatus("Wallet demo ready.");
        }

        private void UpdateBalances()
        {
            SetResult($"Game={IVXNWalletManager.GameBalance}  Global={IVXNWalletManager.GlobalBalance}");
        }

        private long ReadAmount()
        {
            return long.TryParse(_amountField?.value, out long amount) ? amount : 0;
        }

        private async Task RefreshAsync()
        {
            SetBusy(true);
            try
            {
                await IVXNWalletManager.RefreshBalancesAsync();
                UpdateBalances();
                SetStatus("Balances refreshed.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task CreditGameAsync()
        {
            long amount = ReadAmount();
            if (amount <= 0) { SetStatus("Enter a positive amount.", true); return; }
            SetBusy(true);
            try
            {
                bool ok = await IVXNWalletManager.CreditGameAsync(amount, "UITK Wallet Demo");
                UpdateBalances();
                SetStatus(ok ? "Credited game wallet." : "Credit failed.", !ok);
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
            finally { SetBusy(false); }
        }

        private async Task SpendGameAsync()
        {
            long amount = ReadAmount();
            if (amount <= 0) { SetStatus("Enter a positive amount.", true); return; }
            SetBusy(true);
            try
            {
                bool ok = await IVXNWalletManager.TrySpendGameAsync(amount, "UITK Wallet Demo");
                UpdateBalances();
                SetStatus(ok ? "Spent from game wallet." : "Spend failed.", !ok);
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
            finally { SetBusy(false); }
        }

        private async Task CreditGlobalAsync()
        {
            long amount = ReadAmount();
            if (amount <= 0) { SetStatus("Enter a positive amount.", true); return; }
            SetBusy(true);
            try
            {
                bool ok = await IVXNWalletManager.CreditGlobalAsync(amount, "UITK Wallet Demo");
                UpdateBalances();
                SetStatus(ok ? "Credited global wallet." : "Credit failed.", !ok);
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
            finally { SetBusy(false); }
        }

        private async Task SpendGlobalAsync()
        {
            long amount = ReadAmount();
            if (amount <= 0) { SetStatus("Enter a positive amount.", true); return; }
            SetBusy(true);
            try
            {
                bool ok = await IVXNWalletManager.TrySpendGlobalAsync(amount, "UITK Wallet Demo");
                UpdateBalances();
                SetStatus(ok ? "Spent from global wallet." : "Spend failed.", !ok);
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
            finally { SetBusy(false); }
        }
    }
}
