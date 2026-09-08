using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    /// <summary>
    /// Full UITK auth demo: login, signup OTP initiate/confirm, forgot/reset password via APIManager.
    /// </summary>
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Auth Demo")]
    public sealed class IVXUITKAuthDemo : IVXUITKDemoShell
    {
        private VisualElement _loginPanel;
        private VisualElement _signupPanel;
        private VisualElement _otpPanel;
        private VisualElement _forgotPanel;
        private VisualElement _resetPanel;

        private TextField _loginEmail;
        private TextField _loginPassword;
        private TextField _signupEmail;
        private TextField _signupUsername;
        private TextField _signupPassword;
        private TextField _otpCode;
        private TextField _forgotEmail;
        private TextField _resetOtp;
        private TextField _resetPassword;
        private Label _otpHint;

        private string _pendingEmail;
        private string _pendingPassword;
        private string _pendingUsername;
        private CancellationTokenSource _cts;

        protected override void OnUIReady()
        {
            _cts = new CancellationTokenSource();

            _loginPanel = Q<VisualElement>("loginPanel");
            _signupPanel = Q<VisualElement>("signupPanel");
            _otpPanel = Q<VisualElement>("otpPanel");
            _forgotPanel = Q<VisualElement>("forgotPanel");
            _resetPanel = Q<VisualElement>("resetPanel");

            _loginEmail = Q<TextField>("loginEmail");
            _loginPassword = Q<TextField>("loginPassword");
            _signupEmail = Q<TextField>("signupEmail");
            _signupUsername = Q<TextField>("signupUsername");
            _signupPassword = Q<TextField>("signupPassword");
            _otpCode = Q<TextField>("otpCode");
            _forgotEmail = Q<TextField>("forgotEmail");
            _resetOtp = Q<TextField>("resetOtp");
            _resetPassword = Q<TextField>("resetPassword");
            _otpHint = Q<Label>("otpHint");

            BindClick(Q<Button>("loginButton"), () => _ = LoginAsync());
            BindClick(Q<Button>("guestButton"), () => _ = GuestAsync());
            BindClick(Q<Button>("gotoSignupButton"), () => ShowOnly(_signupPanel));
            BindClick(Q<Button>("gotoForgotButton"), () => ShowOnly(_forgotPanel));
            BindClick(Q<Button>("signupInitiateButton"), () => _ = SignupInitiateAsync());
            BindClick(Q<Button>("backToLoginFromSignup"), () => ShowOnly(_loginPanel));
            BindClick(Q<Button>("otpConfirmButton"), () => _ = SignupConfirmAsync());
            BindClick(Q<Button>("otpResendButton"), () => _ = ResendOtpAsync());
            BindClick(Q<Button>("backToSignupFromOtp"), () => ShowOnly(_signupPanel));
            BindClick(Q<Button>("forgotSendButton"), () => _ = ForgotAsync());
            BindClick(Q<Button>("backToLoginFromForgot"), () => ShowOnly(_loginPanel));
            BindClick(Q<Button>("resetConfirmButton"), () => _ = ResetAsync());
            BindClick(Q<Button>("backToForgotFromReset"), () => ShowOnly(_forgotPanel));

            APIManager.TryConfigureUserAuthFromSavedSession(enable: true);
            ShowOnly(_loginPanel);
            SetStatus("Enter credentials or continue as guest.");
        }

        protected override void OnUITeardown()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void ShowOnly(VisualElement panel)
        {
            VisualElement[] panels = { _loginPanel, _signupPanel, _otpPanel, _forgotPanel, _resetPanel };
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] == null)
                    continue;
                if (panels[i] == panel)
                    ShowPanel(panels[i]);
                else
                    panels[i].AddToClassList("ivx-panel--hidden");
            }
        }

        private async Task LoginAsync()
        {
            string email = _loginEmail?.value?.Trim();
            string password = _loginPassword?.value;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                SetStatus("Email and password are required.", true);
                return;
            }

            SetBusy(true);
            SetStatus("Signing in...");
            try
            {
                var resp = await APIManager.LoginAsync(
                    new APIManager.LoginRequest { email = email, password = password, fromDevice = "unity" },
                    configureUserAuthOnSuccess: true,
                    persistSession: true,
                    ct: _cts.Token);

                if (resp != null && resp.status)
                {
                    string name = resp.data?.user?.userName ?? email;
                    SetStatus($"Logged in as {name}");
                }
                else
                {
                    SetStatus(resp?.message ?? "Login failed.", true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task GuestAsync()
        {
            SetBusy(true);
            SetStatus("Creating guest session...");
            try
            {
                var resp = await APIManager.GuestSignupAsync(
                    role: "user",
                    configureUserAuthOnSuccess: true,
                    persistSession: true,
                    ct: _cts.Token);

                if (resp?.data != null)
                    SetStatus("Guest session ready.");
                else
                    SetStatus(resp?.message ?? "Guest signup failed.", true);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SignupInitiateAsync()
        {
            _pendingEmail = _signupEmail?.value?.Trim();
            _pendingPassword = _signupPassword?.value;
            _pendingUsername = _signupUsername?.value?.Trim();

            if (string.IsNullOrWhiteSpace(_pendingEmail) || string.IsNullOrWhiteSpace(_pendingPassword))
            {
                SetStatus("Email and password are required.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(_pendingUsername))
                _pendingUsername = _pendingEmail.Split('@')[0];

            SetBusy(true);
            SetStatus("Sending signup OTP...");
            try
            {
                var resp = await APIManager.SignupInitiateAsync(_pendingEmail, _pendingPassword, "user", _cts.Token);
                if (resp != null && resp.status)
                {
                    if (_otpHint != null)
                        _otpHint.text = $"Enter the OTP sent to {_pendingEmail}";
                    ShowOnly(_otpPanel);
                    SetStatus(resp.message ?? "OTP sent.");
                }
                else
                {
                    SetStatus(resp?.message ?? "Signup initiate failed.", true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ResendOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(_pendingEmail) || string.IsNullOrWhiteSpace(_pendingPassword))
            {
                SetStatus("Missing signup context. Start signup again.", true);
                return;
            }

            SetBusy(true);
            SetStatus("Resending OTP...");
            try
            {
                var resp = await APIManager.SignupInitiateAsync(_pendingEmail, _pendingPassword, "user", _cts.Token);
                SetStatus(resp?.message ?? (resp != null && resp.status ? "OTP resent." : "Resend failed."),
                    resp == null || !resp.status);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SignupConfirmAsync()
        {
            string otp = _otpCode?.value?.Trim();
            if (string.IsNullOrWhiteSpace(otp))
            {
                SetStatus("OTP is required.", true);
                return;
            }

            SetBusy(true);
            SetStatus("Confirming signup...");
            try
            {
                var req = new APIManager.SignupConfirmRequest
                {
                    email = _pendingEmail,
                    password = _pendingPassword,
                    otp = otp,
                    userName = _pendingUsername,
                    role = "user",
                    firstName = "",
                    lastName = ""
                };

                var resp = await APIManager.SignupConfirmAsync(req, configureUserAuthOnSuccess: true, ct: _cts.Token);
                if (resp != null && resp.status)
                {
                    SetStatus("Signup complete. You are signed in.");
                    ShowOnly(_loginPanel);
                }
                else
                {
                    SetStatus(resp?.message ?? "OTP confirm failed.", true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ForgotAsync()
        {
            string email = _forgotEmail?.value?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                SetStatus("Email is required.", true);
                return;
            }

            _pendingEmail = email;
            SetBusy(true);
            SetStatus("Sending reset OTP...");
            try
            {
                var resp = await APIManager.ForgotPasswordAsync(email, _cts.Token);
                if (resp != null && resp.status)
                {
                    ShowOnly(_resetPanel);
                    SetStatus(resp.message ?? "Reset OTP sent.");
                }
                else
                {
                    SetStatus(resp?.message ?? "Forgot password failed.", true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ResetAsync()
        {
            string otp = _resetOtp?.value?.Trim();
            string newPassword = _resetPassword?.value;
            if (string.IsNullOrWhiteSpace(_pendingEmail) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
            {
                SetStatus("Email context, OTP, and new password are required.", true);
                return;
            }

            SetBusy(true);
            SetStatus("Resetting password...");
            try
            {
                var resp = await APIManager.ResetPasswordAsync(_pendingEmail, otp, newPassword, _cts.Token);
                if (resp != null && resp.status)
                {
                    ShowOnly(_loginPanel);
                    SetStatus(resp.message ?? "Password reset. You can sign in.");
                }
                else
                {
                    SetStatus(resp?.message ?? "Reset failed.", true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
