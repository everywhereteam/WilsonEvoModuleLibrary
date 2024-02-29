using FluentResults;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services;

public interface IConfigStorageService
{
    Result<T> GetConfiguration<T>(SessionData session);
}