namespace EsiaUserGenerator.Exception;
using System;

public class EsiaRequestException(string message) : Exception(message);