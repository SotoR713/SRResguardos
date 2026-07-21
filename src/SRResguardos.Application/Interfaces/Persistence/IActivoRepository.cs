public interface IActivoRepository
{
	Task<Activo?> ObtenerPorIdAsync(int idActivo);

	Task ActualizarAsync(Activo activo);
}