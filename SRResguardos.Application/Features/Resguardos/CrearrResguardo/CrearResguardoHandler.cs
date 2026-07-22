using SRResguardos.Application.Interfaces.Persistence;

namespace SRResguardos.Application.Features.Resguardos.CrearResguardo;

public class CrearResguardoHandler
{
	private readonly IActivoRepository _activoRepository;

	public CrearResguardoHandler(IActivoRepository activoRepository)
	{
		_activoRepository = activoRepository;
	}

	public void Handle(CrearResguardoCommand command)
	{

	}
}