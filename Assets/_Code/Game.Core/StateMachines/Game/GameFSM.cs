using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Core.StateMachines.Game
{
	public class GameFSM
	{
		public enum States { Init, Title, SelectLevel, Intro, LoadLevel, Gameplay, Ending, Credits, Quit }
		public enum Triggers { Done, StartGame, Won, Lost, Retry, NextLevel, LevelSelected, LevelSelectionRequested, CreditsRequested, Quit }

		private readonly bool _debug;
		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;

		public GameFSM(bool debug, GameSingleton game)
		{
			Assert.IsNotNull(game);

			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Init, new GameInitState { FSM = this } },
				{ States.Title, new GameTitleState { FSM = this } },
				{ States.SelectLevel, new GameSelectLevelState { FSM = this } },
				{ States.Intro, new GameIntroState { FSM = this } },
				{ States.LoadLevel, new GameLoadLevelState { FSM = this } },
				{ States.Gameplay, new GameGameplayState { FSM = this } },
				{ States.Ending, new GameEndingState { FSM = this } },
				{ States.Credits, new GameCreditsState { FSM = this } },
				{ States.Quit, new GameQuitState { FSM = this } },
			};

			_machine = new StateMachine<States, Triggers>(States.Init);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Init)
				.Permit(Triggers.Done, States.Title);

			_machine.Configure(States.Title)
				.Permit(Triggers.StartGame, States.LoadLevel)
				.Permit(Triggers.LevelSelected, States.LoadLevel)
				.Permit(Triggers.LevelSelectionRequested, States.SelectLevel)
				.Permit(Triggers.CreditsRequested, States.Credits)
				.Permit(Triggers.Quit, States.Quit);

			_machine.Configure(States.SelectLevel)
				.Permit(Triggers.LevelSelected, States.LoadLevel)
				.Permit(Triggers.Quit, States.Title);

			_machine.Configure(States.Intro)
				.Permit(Triggers.Done, States.LoadLevel);

			_machine.Configure(States.LoadLevel)
				.Permit(Triggers.Done, States.Gameplay);

			_machine.Configure(States.Gameplay)
				.Permit(Triggers.Won, States.Ending)
				.Permit(Triggers.Quit, States.Quit)
				.Permit(Triggers.NextLevel, States.LoadLevel)
				.Permit(Triggers.LevelSelectionRequested, States.SelectLevel)
				.Permit(Triggers.Retry, States.LoadLevel);

			_machine.Configure(States.Ending)
				.Permit(Triggers.Done, States.Credits);

			_machine.Configure(States.Credits)
				.Permit(Triggers.Done, States.Title);

			_currentState = _states[_machine.State];
		}

		public UniTask Start() => _currentState.Enter();

		public void Tick() => _currentState?.Tick();

		public void FixedTick() => _currentState?.FixedTick();

		public void Fire(Triggers trigger)
		{
			if (_machine.CanFire(trigger))
				_machine.Fire(trigger);
			else
				Debug.LogWarning("GameFSM: Invalid transition " + _machine.State + " -> " + trigger);
		}

		private async void OnTransitioned(StateMachine<States, Triggers>.Transition transition)
		{
			if (_currentState != null)
			{
				Log($"GameFSM: {transition.Source}.Exit (before)");
				await _currentState.Exit();
				Log($"GameFSM: {transition.Source}.Exit (after)");
			}

			if (_states.ContainsKey(transition.Destination) == false)
				LogError("GameFSM: Missing state class for: " + transition.Destination);

			_currentState = _states[transition.Destination];
			Log($"GameFSM: {transition.Source} -> {transition.Destination}");

			Log($"GameFSM: {transition.Destination}.Enter (before)");
			await _currentState.Enter();
			Log($"GameFSM: {transition.Destination}.Enter (after)");
		}

		private void Log(object message)
		{
			if (_debug == false)
				return;
			Debug.Log(message);
		}

		private void LogError(object message)
		{
			if (_debug == false)
				return;
			Debug.LogError(message);
		}
	}
}
