using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Data;
using WebApi_Identity_Provider_DotNet.Models;

namespace WebApi_Identity_Provider_DotNet.Services
{
    public class CredentialService
    {
        private ApplicationDbContext _context;

        public CredentialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool RegisterCredential(string userId, string publicKey, string publicKeyHash, string deviceName)
        {
            _context.Credentials.Add(new Credential
            {
                UserId = userId,
                PublicKey = publicKey,
                PublicKeyHash = publicKeyHash,
                DeviceName = deviceName
            });
            _context.SaveChanges();

            return true;
        }

        public bool RemoveCredential(string userId, string publicKeyHash)
        {
            var credential = _context.Credentials.SingleOrDefault(c => c.UserId == userId && c.PublicKeyHash == publicKeyHash);
            _context.Credentials.Remove(credential);
            _context.SaveChanges();

            return true;
        }

        public JsonWebKey GetPublicKeyForUser(string userId, string publicKeyHash, out string challenge)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.UserId == userId && c.PublicKeyHash == publicKeyHash);

            // Each challenge is valid only once. Prevent replay attack.
            challenge = credential.ActiveChallenge;
            credential.ActiveChallenge = null;
            _context.Entry(credential).State = EntityState.Modified;
            _context.SaveChanges();

            return JsonConvert.DeserializeObject<JsonWebKey>(credential.PublicKey);
        }

        public bool SetActiveChallenge(string userId, string publicKeyHash, string challenge)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.UserId == userId && c.PublicKeyHash == publicKeyHash);
            credential.ActiveChallenge = challenge;
            _context.Entry(credential).State = EntityState.Modified;
            _context.SaveChanges();

            return true;
        }
    }
}
