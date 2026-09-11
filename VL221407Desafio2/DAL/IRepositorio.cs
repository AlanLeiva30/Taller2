namespace VL221407Desafio2.DAL;

public interface IRepositorio<T> where T : class
{
    Task<IEnumerable<T>> ListarAsync(CancellationToken ct = default);
    Task<T?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<int> CrearAsync(T entidad, CancellationToken ct = default);
    Task<bool> ActualizarAsync(int id, T entidad, CancellationToken ct = default);
    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}

public interface IInscripcionRepositorio : IRepositorio<VL221407Desafio2.Entities.Inscripcion>
{
    Task<bool> ExisteAsync(int estudiante, int curso, int excluirId = 0, CancellationToken ct = default);
}
