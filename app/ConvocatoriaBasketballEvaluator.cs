using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PeachPied.Demo
{
    public sealed class Aspirante
    {
        public DateTime FechaNacimiento { get; init; }
        public decimal PesoKg { get; init; }
        public decimal EstaturaCm { get; init; }
    }

    public sealed class ResultadoAspirante
    {
        public Aspirante Aspirante { get; init; } = new Aspirante();
        public int Edad { get; init; }
        public decimal Imc { get; init; }
        public string ClasificacionImc { get; init; } = string.Empty;
        public bool Aceptado { get; init; }
        public IReadOnlyList<string> MotivosNoAceptacion { get; init; } = Array.Empty<string>();
    }

    public sealed class ResumenConvocatoria
    {
        public IReadOnlyList<ResultadoAspirante> Resultados { get; init; } = Array.Empty<ResultadoAspirante>();
        public int TotalAspirantes => Resultados.Count;
        public int TotalAceptados => Resultados.Count(r => r.Aceptado);
        public int TotalNoAceptados => TotalAspirantes - TotalAceptados;

        public decimal PromedioEdadAceptados => PromedioAceptados(r => r.Edad);
        public decimal PromedioPesoAceptados => PromedioAceptados(r => r.Aspirante.PesoKg);
        public decimal PromedioEstaturaCmAceptados => PromedioAceptados(r => r.Aspirante.EstaturaCm);
        public decimal PromedioImcAceptados => PromedioAceptados(r => r.Imc);

        public decimal PorcentajeAceptados => TotalAspirantes == 0 ? 0 : (decimal)TotalAceptados * 100m / TotalAspirantes;
        public decimal PorcentajeNoAceptados => TotalAspirantes == 0 ? 0 : (decimal)TotalNoAceptados * 100m / TotalAspirantes;

        private decimal PromedioAceptados(Func<ResultadoAspirante, decimal> selector)
        {
            var aceptados = Resultados.Where(r => r.Aceptado).ToArray();
            return aceptados.Length == 0 ? 0 : aceptados.Average(selector);
        }
    }

    public static class ConvocatoriaBasketballEvaluator
    {
        public static ResumenConvocatoria Evaluar(IEnumerable<Aspirante> aspirantes, DateTime? fechaReferencia = null)
        {
            var referencia = fechaReferencia?.Date ?? DateTime.Today;
            var resultados = aspirantes.Select(a => EvaluarAspirante(a, referencia)).ToArray();

            return new ResumenConvocatoria { Resultados = resultados };
        }

        private static ResultadoAspirante EvaluarAspirante(Aspirante aspirante, DateTime fechaReferencia)
        {
            var edad = CalcularEdad(aspirante.FechaNacimiento, fechaReferencia);
            var estaturaMts = aspirante.EstaturaCm / 100m;
            var imc = aspirante.PesoKg / (estaturaMts * estaturaMts);
            var clasificacionImc = ClasificarImc(imc);

            var motivos = new List<string>();
            if (edad < 20 || edad > 30)
            {
                motivos.Add("No cumple el requisito de edad (20 a 30 años).");
            }

            if (aspirante.EstaturaCm <= 190m)
            {
                motivos.Add("No cumple el requisito de estatura (> 190 cm).");
            }

            if (!string.Equals(clasificacionImc, "Normopeso", StringComparison.OrdinalIgnoreCase))
            {
                motivos.Add($"No cumple el requisito de IMC (debe ser Normopeso y es {clasificacionImc}).");
            }

            return new ResultadoAspirante
            {
                Aspirante = aspirante,
                Edad = edad,
                Imc = decimal.Round(imc, 2, MidpointRounding.AwayFromZero),
                ClasificacionImc = clasificacionImc,
                Aceptado = motivos.Count == 0,
                MotivosNoAceptacion = motivos
            };
        }

        private static int CalcularEdad(DateTime fechaNacimiento, DateTime fechaReferencia)
        {
            var edad = fechaReferencia.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > fechaReferencia.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }

        public static string ClasificarImc(decimal imc)
        {
            if (imc < 16.5m) return "Bajo peso severo";
            if (imc < 18.5m) return "Bajo peso";
            if (imc <= 24.9m) return "Normopeso";
            if (imc <= 26.9m) return "Sobrepeso grado I";
            if (imc <= 29.9m) return "Sobrepeso grado II";
            if (imc <= 34.9m) return "Obesidad de tipo I";
            if (imc <= 39.9m) return "Obesidad de tipo II";
            if (imc <= 49.9m) return "Obesidad de tipo III (mórbida)";
            return "Obesidad extrema";
        }

        // Algoritmo de consola opcional para leer exactamente 100 aspirantes.
        public static void EjecutarDesdeConsola()
        {
            var aspirantes = new List<Aspirante>();

            for (var i = 1; i <= 100; i++)
            {
                Console.WriteLine($"Aspirante #{i}");

                Console.Write("Fecha de nacimiento (yyyy-MM-dd): ");
                var fecha = DateTime.ParseExact(Console.ReadLine() ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                Console.Write("Peso en kg: ");
                var peso = decimal.Parse(Console.ReadLine() ?? string.Empty, CultureInfo.InvariantCulture);

                Console.Write("Estatura en cm: ");
                var estaturaCm = decimal.Parse(Console.ReadLine() ?? string.Empty, CultureInfo.InvariantCulture);

                aspirantes.Add(new Aspirante
                {
                    FechaNacimiento = fecha,
                    PesoKg = peso,
                    EstaturaCm = estaturaCm
                });
            }

            var resumen = Evaluar(aspirantes);

            Console.WriteLine($"Aceptados: {resumen.TotalAceptados}");
            Console.WriteLine($"No aceptados: {resumen.TotalNoAceptados}");

            foreach (var resultado in resumen.Resultados.Where(r => !r.Aceptado))
            {
                Console.WriteLine("No aceptado por:");
                foreach (var motivo in resultado.MotivosNoAceptacion)
                {
                    Console.WriteLine($" - {motivo}");
                }
            }

            Console.WriteLine($"Promedio edad (aceptados): {resumen.PromedioEdadAceptados:F2}");
            Console.WriteLine($"Promedio peso (aceptados): {resumen.PromedioPesoAceptados:F2}");
            Console.WriteLine($"Promedio estatura cm (aceptados): {resumen.PromedioEstaturaCmAceptados:F2}");
            Console.WriteLine($"Promedio IMC (aceptados): {resumen.PromedioImcAceptados:F2}");
            Console.WriteLine($"% aceptados: {resumen.PorcentajeAceptados:F2}%");
            Console.WriteLine($"% no aceptados: {resumen.PorcentajeNoAceptados:F2}%");
        }
    }
}
