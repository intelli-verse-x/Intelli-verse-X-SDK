using IntelliVerseX.Editor;
using NUnit.Framework;
using UnityEditor;

namespace IntelliVerseX.Tests.Editor
{
    [TestFixture]
    public class IVXConnectWizardValidationTests
    {
        [Test]
        public void IsValidEmail_AcceptsNormalAddresses()
        {
            Assert.IsTrue(IVXConnectWizardValidation.IsValidEmail("dev@example.com"));
            Assert.IsTrue(IVXConnectWizardValidation.IsValidEmail("  a.b+c@mail.co  "));
        }

        [Test]
        public void IsValidEmail_RejectsJunk()
        {
            Assert.IsFalse(IVXConnectWizardValidation.IsValidEmail(""));
            Assert.IsFalse(IVXConnectWizardValidation.IsValidEmail("not-an-email"));
            Assert.IsFalse(IVXConnectWizardValidation.IsValidEmail("@missing.local"));
        }

        [Test]
        public void IsValidUuid_AcceptsCanonicalUuid()
        {
            Assert.IsTrue(IVXConnectWizardValidation.IsValidUuid("86fe6671-11e5-4c62-842c-aab377af4fcd"));
            Assert.IsTrue(IVXConnectWizardValidation.IsValidUuid("  86FE6671-11E5-4C62-842C-AAB377AF4FCD  "));
        }

        [Test]
        public void IsValidUuid_RejectsNonUuid()
        {
            Assert.IsFalse(IVXConnectWizardValidation.IsValidUuid(""));
            Assert.IsFalse(IVXConnectWizardValidation.IsValidUuid("not-a-uuid"));
            Assert.IsFalse(IVXConnectWizardValidation.IsValidUuid("86fe6671-11e5-4c62-842c"));
        }

        [Test]
        public void TryValidateGameName_EnforcesBounds()
        {
            Assert.IsFalse(IVXConnectWizardValidation.TryValidateGameName("a", out _));
            Assert.IsTrue(IVXConnectWizardValidation.TryValidateGameName("ab", out _));
            Assert.IsTrue(IVXConnectWizardValidation.TryValidateGameName("testingsdk", out _));
            Assert.IsFalse(IVXConnectWizardValidation.TryValidateGameName(new string('x', 81), out _));
            Assert.IsFalse(IVXConnectWizardValidation.TryValidateGameName("bad\nname", out _));
        }

        [Test]
        public void TryValidateSignIn_RequiresEmailAndPassword()
        {
            Assert.IsFalse(IVXConnectWizardValidation.TryValidateSignIn("", "secret", out _));
            Assert.IsFalse(IVXConnectWizardValidation.TryValidateSignIn("dev@example.com", "", out _));
            Assert.IsTrue(IVXConnectWizardValidation.TryValidateSignIn("dev@example.com", "secret", out _));
        }

        [Test]
        public void RememberRecentGame_RoundTripsInEditorPrefs()
        {
            string key = IVXConnectWizardValidation.PrefRecentGames;
            string previous = EditorPrefs.GetString(key, string.Empty);
            try
            {
                EditorPrefs.DeleteKey(key);
                IVXConnectWizardValidation.RememberRecentGame(
                    "86fe6671-11e5-4c62-842c-aab377af4fcd",
                    "testingsdk");

                var loaded = IVXConnectWizardValidation.LoadRecentGames();
                Assert.AreEqual(1, loaded.Length);
                Assert.AreEqual("86fe6671-11e5-4c62-842c-aab377af4fcd", loaded[0].id);
                Assert.AreEqual("testingsdk", loaded[0].name);

                var labels = IVXConnectWizardValidation.RecentGamePopupLabels(loaded);
                Assert.AreEqual(2, labels.Length);
                StringAssert.Contains("testingsdk", labels[1]);
            }
            finally
            {
                if (string.IsNullOrEmpty(previous))
                    EditorPrefs.DeleteKey(key);
                else
                    EditorPrefs.SetString(key, previous);
            }
        }
    }
}
