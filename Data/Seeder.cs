using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MedicalSystem.Entities;
using MedicalSystem.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MedicalSystem.Services;

namespace MedicalSystem.Data
{
    public class Seeder
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<User> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;
        private readonly IConfiguration _config;
        private readonly IMedicalOfficerService medicalOfficer;
        private readonly IPatientService patientService;

        public Seeder(AppDbContext ctx,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config, IPatientService patientService, IMedicalOfficerService medicalOfficer)
        {
            _ctx = ctx;
            _userMgr = userManager;
            _roleMgr = roleManager;
            _config = config;
            this.patientService = patientService;
            this.medicalOfficer = medicalOfficer;
        }


        public async Task SeedIt()
        {
            //_ctx.Database.EnsureCreated();
            if (_ctx.Database.GetPendingMigrations().Any())
            {
                _ctx.Database.Migrate();
            }

            try
            {
                //seed roles table
                var roles = new string[] {"Admin","Role1","Role2","Role3","Role4","Role5","Role6","Role7","Role8","Role9"};
                if (!_roleMgr.Roles.Any())
                {
                    foreach (var role in roles)
                    {
                        await _roleMgr.CreateAsync(new IdentityRole(role));
                    }
                }

                // seed users table
                if (!_userMgr.Users.Any())
                {
                    // read seeds from dedicated paths
                    var usersData = File.ReadAllText(_config.GetSection("SeedDataPaths:User").Value);

                    // Deserialize seed data
                    var usersToSeed = JsonConvert.DeserializeObject<List<User>>(usersData);

                    var counter = 0;
                    foreach (var user in usersToSeed)
                    {
                        Random rand = new Random();
                        var _roles = new string[] {"Role2","Role3","Role4","Role5","Role6","Role7","Role8","Role9" };
                        var randomRoles = _roles.OrderBy(x => rand.Next()).Take(4);
                        var res = await _userMgr.CreateAsync(user, "P@ssw0rd");
                        if (res.Succeeded)
                        {
                            if(user.UserType == UserType.Admin)
                            {
                                await _userMgr.AddToRoleAsync(user, "Admin");
                            }else if(user.UserType == UserType.Patient)
                            {
                                await patientService.CreateUser(new Patient()
                                {
                                    Id = user.Id,
                                    UserId = user.Id
                                }, default);
                                user.Roles = randomRoles.ToList();
                                await _userMgr.AddToRoleAsync(user, "Role1");
                            }
                            else
                            {
                                await medicalOfficer.CreateUser(new MedicalOfficer()
                                {
                                    Id = user.Id,
                                    UserId = user.Id
                                }, default);
                                user.Roles = randomRoles.ToList();
                                await _userMgr.AddToRolesAsync(user, randomRoles);
                            }
                        }

                        counter++;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // log err
            }
            catch (DbException)
            {
                //log err
            }

        }
    }

}