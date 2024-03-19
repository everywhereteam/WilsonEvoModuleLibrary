using FluentResults;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Services;

public interface IConfigStorageService
{
    Result<T> GetConfiguration<T>(SessionData session);
}