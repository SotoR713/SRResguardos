public interface IEmpleadoRepository
{
    Task<Empleado?> ObtenerPorIdAsync(int idEmpleado);
}