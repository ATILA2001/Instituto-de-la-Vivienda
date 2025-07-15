using System;
using System.Linq;
using Dominio;
using Negocio;

namespace TestEF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PRUEBA DE FUNCIONALIDAD EF ===");
            
            try
            {
                // Instanciar la clase de negocio EF
                var calculoNegocio = new CalculoRedeterminacionNegocioEF();
                
                // Prueba 1: Verificar conexión
                Console.WriteLine("1. Probando conexión...");
                var resultadoConexion = calculoNegocio.ProbarConexionYDatos();
                Console.WriteLine($"   Resultado: {resultadoConexion}");
                
                // Prueba 2: Obtener lista completa de certificados
                Console.WriteLine("\n2. Obteniendo lista de certificados...");
                var certificados = calculoNegocio.ListarCertReliq();
                Console.WriteLine($"   Total de certificados obtenidos: {certificados?.Count ?? 0}");
                
                if (certificados != null && certificados.Any())
                {
                    Console.WriteLine("\n3. Analizando propiedades calculadas...");
                    
                    // Tomar los primeros 3 certificados para análisis
                    var muestra = certificados.Take(3);
                    
                    foreach (var cert in muestra)
                    {
                        Console.WriteLine($"\n   --- Certificado ID: {cert.Id} ---");
                        Console.WriteLine($"   ExpedientePago: {cert.ExpedientePago ?? "NULL"}");
                        Console.WriteLine($"   Contrata: {cert.Contrata ?? "NULL"}");
                        Console.WriteLine($"   Empresa: {cert.Empresa ?? "NULL"}");
                        Console.WriteLine($"   Estado: {cert.Estado ?? "NULL"}");
                        Console.WriteLine($"   Porcentaje: {cert.Porcentaje:F2}%");
                        Console.WriteLine($"   Sigaf: {cert.Sigaf?.ToString("C") ?? "NULL"}");
                        Console.WriteLine($"   BuzonSade: {cert.BuzonSade ?? "NULL"}");
                        Console.WriteLine($"   FechaSade: {cert.FechaSade?.ToString("dd/MM/yyyy") ?? "NULL"}");
                        Console.WriteLine($"   Expediente: {cert.Expediente ?? "NULL"}");
                        
                        // Verificar datos del autorizante
                        if (cert.Autorizante != null)
                        {
                            Console.WriteLine($"   Autorizante.CodigoAutorizante: {cert.Autorizante.CodigoAutorizante ?? "NULL"}");
                            Console.WriteLine($"   Autorizante.Detalle: {cert.Autorizante.Detalle ?? "NULL"}");
                            
                            if (cert.Autorizante.Obra != null)
                            {
                                Console.WriteLine($"   Obra.Descripcion: {cert.Autorizante.Obra.Descripcion ?? "NULL"}");
                                Console.WriteLine($"   Obra.Numero: {cert.Autorizante.Obra.Numero?.ToString() ?? "NULL"}");
                                Console.WriteLine($"   Obra.Anio: {cert.Autorizante.Obra.Anio?.ToString() ?? "NULL"}");
                                
                                if (cert.Autorizante.Obra.Area != null)
                                    Console.WriteLine($"   Area.Nombre: {cert.Autorizante.Obra.Area.Nombre ?? "NULL"}");
                                
                                if (cert.Autorizante.Obra.Barrio != null)
                                    Console.WriteLine($"   Barrio.Nombre: {cert.Autorizante.Obra.Barrio.Nombre ?? "NULL"}");
                                
                                if (cert.Autorizante.Obra.Contrata != null)
                                    Console.WriteLine($"   Contrata.Nombre: {cert.Autorizante.Obra.Contrata.Nombre ?? "NULL"}");
                                
                                if (cert.Autorizante.Obra.Empresa != null)
                                    Console.WriteLine($"   Empresa.Nombre: {cert.Autorizante.Obra.Empresa.Nombre ?? "NULL"}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"   *** AUTORIZANTE ES NULL ***");
                        }
                    }
                    
                    // Prueba 4: Verificar cálculos específicos
                    Console.WriteLine("\n4. Verificando estados...");
                    var estadosCounts = certificados.GroupBy(c => c.Estado)
                                                   .ToDictionary(g => g.Key, g => g.Count());
                    
                    foreach (var estado in estadosCounts)
                    {
                        Console.WriteLine($"   {estado.Key}: {estado.Value} certificados");
                    }
                }
                else
                {
                    Console.WriteLine("   No se obtuvieron certificados o la lista es null");
                }
                
                Console.WriteLine("\n=== PRUEBA COMPLETADA ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== ERROR EN PRUEBA ===");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Tipo: {ex.GetType().Name}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\nInnerException: {ex.InnerException.Message}");
                }
            }
            
            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
