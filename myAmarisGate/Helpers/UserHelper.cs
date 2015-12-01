using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Configuration;

namespace AmarisGate.Helpers
{
    public static class UserHelper
    {
        private static readonly IDictionary<string, string> AuthentificationFaker = new Dictionary<string, string>();

        public static bool AuthentificationFaked { get { return AuthentificationFaker.ContainsKey(RealUserName); } }

        public static string RealUserName
        {
            get
            {
                var username = ClaimsPrincipal.Current.Identity.Name;

                return username;
            }
        }

        public static void Faker(string login)
        {
            if (String.IsNullOrWhiteSpace(login))
            {
                if (AuthentificationFaker.ContainsKey(RealUserName))
                {
                    AuthentificationFaker.Remove(RealUserName);
                }
            }
            else
            {
                if (!AuthentificationFaker.ContainsKey(RealUserName))
                {
                    AuthentificationFaker.Add(RealUserName, login);
                }
                else if (AuthentificationFaker[RealUserName] != login)
                {
                    AuthentificationFaker.Remove(RealUserName);
                    AuthentificationFaker.Add(RealUserName, login);
                }
            }

        }

        public static string UserName()
        {
            return AuthentificationFaker.ContainsKey(RealUserName)
                       ? AuthentificationFaker[RealUserName]
                       : RealUserName;
        }
    }
}