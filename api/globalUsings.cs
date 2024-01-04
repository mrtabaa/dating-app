global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using api.DTOs;
global using api.Helpers;
global using api.Extensions;
global using api.Extensions.Validations;
global using api.Interfaces;
global using api.Models;
global using api.Models.Helpers;
global using api.Repositories;
global using api.Services;
global using api.Settings;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using MongoDB.Bson;
global using MongoDB.Bson.Serialization.Attributes;
global using MongoDB.Driver;
global using MongoDB.Driver.Linq;
global using api.Middleware;