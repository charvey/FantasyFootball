using Objects.Fantasy;

namespace Simulation.Fantasy
{
	public interface DraftParticipantSimulator
	{
		Player DeterminePick(DraftState state);
	}
}
