using System.Collections.Generic;
using System.Linq;

namespace CodeLuau
{
    public class Speaker
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? YearsExperience { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<Session> Sessions { get; set; }

        public RegisterResponse Register(IRepository repository)
        {
            var registerError = ValidateRegistration();
            if (registerError != null) return new RegisterResponse(registerError);
            
            double fee = CalculateFee();

            int speakerId = repository.SaveSpeaker(this);
            return new RegisterResponse(speakerId);
        }

        private RegisterError? ValidateRegistration()
        {
            var error = ValidateData();
            if (error != null) return error;

            bool IsQualified = IsExceptional() || !HasRedFlags();
            if (!IsQualified) return RegisterError.SpeakerDoesNotMeetStandards;
            
            var approvalError = ApproveSessions();
            if (approvalError == null) return null;
            return approvalError;
        }

        private RegisterError? ApproveSessions()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !SessionIsAboutOldTechnology(session);
            }
            if (Sessions.Any(s => s.Approved)) return null;
            return RegisterError.NoSessionsApproved;
        }

        private double CalculateFee()
        {
            if (YearsExperience <= 1) RegistrationFee = 500;
            else if (YearsExperience <= 3) RegistrationFee = 250;
            else if (YearsExperience <= 5) RegistrationFee = 100;
            else if (YearsExperience <= 9) RegistrationFee = 50;
            else RegistrationFee = 0;

            return RegistrationFee;
        }
        
        private bool SessionIsAboutOldTechnology(Session session)
        {
            var oldTechnologies = new List<string> { "Cobol", "Punch Cards", "Commodore", "VBScript" };
            foreach (var oldTech in oldTechnologies)
            {
                if (session.Title.Contains(oldTech) || session.Description.Contains(oldTech)) return true;
            }
            return false;
        }

        private bool HasRedFlags()
        {
            string emailDomain = Email.Split('@').Last();
            var OldDomain = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };
            return (OldDomain.Contains(emailDomain) || ((Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)));
        }

        private bool IsExceptional()
        {
            var ListEmployers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            return (YearsExperience>10 || HasBlog || Certifications.Count >3 || ListEmployers.Contains(Employer));
        }

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrEmpty(FirstName)) return RegisterError.FirstNameRequired;
            if (string.IsNullOrEmpty(LastName)) return RegisterError.LastNameRequired;
            if (string.IsNullOrEmpty(Email)) return RegisterError.EmailRequired;
            if (!Sessions.Any()) return RegisterError.NoSessionsProvided;
            return null;
        }
    }
}