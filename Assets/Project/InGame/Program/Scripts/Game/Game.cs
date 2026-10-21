#nullable enable

using BattleField.Script.Progress;
using Common.QRCode;
using InGame;
using InGame.Turn;
using Project.InGame.Program.Scripts.DebugSheet;
using UnityEngine;

namespace Project.InGame.Program.Scripts.Game
{
    public class Game : MonoBehaviour
    {
        private GameSettings _gameSettings = null!;
        private ScannerModel _scannerModel = null!;
        [SerializeField] private Scanner scanner = null!;
        [SerializeField] private TurnPresenter turn = null!;
        [SerializeField] private Progressor progressor = null!;
        [SerializeField] private InGamePage debugController = null!;

        private void Start()
        {
            _gameSettings = new GameSettings();
            _scannerModel = new ScannerModel();

            scanner.Initialize(_scannerModel);
            turn.Initialize(_gameSettings.Player, _gameSettings.OpponentPlayer, _scannerModel, _gameSettings.MaxTurn);
            progressor.Initialize(_gameSettings);

#if !EXCLUDE_UNITY_DEBUG_SHEET
            debugController.Initialize(_scannerModel);
#endif
        }
    }
}