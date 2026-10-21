using BattleField.Script.Battle.Draw.Judge;
using BattleField.Script.Battle.Lose.Judge;
using BattleField.Script.Battle.Win.Judge;
using BattleField.Script.HpBar.Model;
using BattleField.Script.Judge;
using BattleField.Script.TimelineManagge;
using BattleField.Script.EnemyData;
using Common.MasterData;
using Cysharp.Threading.Tasks;
using InGame;
using InGame.Player;
using InGame.Turn;
using System.Linq;
using System.Threading;
using Project.BattleField.Script.GameEnd;
using Project.GameEnd.Program.Scripts;
using Project.InGame.Program.Scripts.Audio;
using Project.InGame.Program.Scripts.Tehuda;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace BattleField.Script.Progress
{
    public class Progressor : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private TurnPresenter _turnPresenter;

        [SerializeField] private TimelineManagger _round_Timeline;
        //[SerializeField] private TimelineManagger _janken_Timeline;
        //[SerializeField] private JankenSelector _playerJankenSelector;
        //[SerializeField] private JankenSelector _enemyJankenSelector;
        [SerializeField] private TextEffect _damageTextEffect;
        [SerializeField] private Round _round;
        [SerializeField] private TimelineManagger _damage_Timeline;
        [SerializeField] private Win _winAction;
        [SerializeField] private Draw _drawAction;
        [SerializeField] private Lose _loseAction;
        [SerializeField] private HpBarModel _playerhp;
        [SerializeField] private HpBarModel _enemyhp;
        [SerializeField] private AudioSource roundBgmAudioSource = null!;
        [SerializeField] private AudioSource finalRoundBgmAudioSource = null!;
        [SerializeField] private RoundAudioSource roundSeAudioSource = null!;
        [SerializeField] private PlayableDirector fightDirector = null!;
        [SerializeField] private SpriteRenderer _playerIcon;
        [SerializeField] private SpriteRenderer _enemyIcon;
        [SerializeField] private TehudaPresenter tehuda = null!;

        //[SerializeField] private GameEndText _gameEndText;
        [SerializeField] private int turn;
        [SerializeField] private TimelineManagger _pCardSummon;
        [SerializeField] private TimelineManagger _eCardSummon;
        [SerializeField] private SpriteRenderer _pCard;
        [SerializeField] private SpriteRenderer _eCard;

        private Card _playerCard;
        private Card _enemyCard;
        private string _enemyCardID;
        private int _turn = 1;
        private int _maxTurn;
        private PlayerPresenter _playerPresenter;
        private PlayerPresenter _enemyPresenter;
        private GameSettings _gameSettings;
        private ViewJudge _viewJudge;
        private EnemyDataSets _enemyDataSets;

        public void Initialize(GameSettings gameSettings)
        {
            _enemyDataSets = new EnemyDataSets();
            _viewJudge = new ViewJudge();
            _gameSettings = gameSettings;
            _playerPresenter = gameSettings.Player;
            _enemyPresenter =　gameSettings.OpponentPlayer;
            _maxTurn = _gameSettings.MaxTurn;
            CancellationToken ct = this.GetCancellationTokenOnDestroy();
            Role(ct).Forget();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void SetPlayer() {
            _playerIcon.sprite = Resources.Load<Sprite>("Images/"+_playerCard.ImageId);
        }

        public void SetEnemy() {
            _enemyIcon.sprite = Resources.Load<Sprite>("Images/"+_enemyCard.ImageId);
        }

        private async UniTaskVoid Role(CancellationToken ct)
        {
            var _enemyIdSet = _enemyDataSets.EnemySetReceiver();
            foreach(var _enemyId in _enemyIdSet) {
                Debug.Log(_enemyId);
            } Debug.Log("");
            while (_turn <= _maxTurn)
            {
                if (_turn == _maxTurn)
                {
                    roundBgmAudioSource.Stop();
                    finalRoundBgmAudioSource.Play();
                }
                else if(_turn == 1)
                {
                    roundBgmAudioSource.Play();
                }

                roundSeAudioSource.Play(_turn);
                _enemyCardID = _enemyDataSets.EnemyIDReceiver(_enemyIdSet, _enemyPresenter.Cards);
                Debug.Log(_enemyCardID);
                _playerIcon.sprite = Resources.Load<Sprite>("Images/null");
                _enemyIcon.sprite = Resources.Load<Sprite>("Images/null");
                _round.RoundCount(_turn);
                _round_Timeline.TimelinePlay();
                await UniTask.WaitUntil(() => _round_Timeline.IsDone());
                tehuda.SetImages(_enemyIdSet, _enemyPresenter.Cards);
                await UniTask.WhenAll(_turnPresenter.HandleTurn(ct));
                _playerCard = _playerPresenter.Cards.Last();
                var result = _enemyPresenter.SetCurrentCard(_enemyCardID);
                _enemyCard = _enemyPresenter.Cards.Last();
                _pCardSummon.TimelinePlay();
                _pCard.sprite = Resources.Load<Sprite>("Cards/"+_playerCard.ImageId);
                await UniTask.WaitUntil(() => _pCardSummon.IsDone());
                _eCardSummon.TimelinePlay();
                _eCard.sprite = Resources.Load<Sprite>("Cards/"+_enemyCard.ImageId);
                await UniTask.WaitUntil(() => _eCardSummon.IsDone());
                JudgementType judge = _viewJudge.JankenJudge(_playerCard.Hand, _enemyCard.Hand);
                fightDirector.Play();
                await UniTask.WaitUntil(() => fightDirector.state == PlayState.Paused, cancellationToken: ct);
                //_janken_Timeline.TimelinePlay();
                //_playerJankenSelector.SetOptions(_playerCard.Hand);
                //_enemyJankenSelector.SetOptions(_enemyCard.Hand);
                //await UniTask.WaitUntil(() => _janken_Timeline.IsDone());
                if (judge == JudgementType.Win) await UniTask.WhenAll(_winAction.WinProcess(_playerCard, _enemyCard, ct));
                else if (judge == JudgementType.Draw) await UniTask.WhenAll(_drawAction.DrawProcess(_playerCard, _enemyCard, ct));
                else await UniTask.WhenAll(_loseAction.LoseProcess(_playerCard, _enemyCard, ct));
                if (_playerhp.Health <= 0 || _enemyhp.Health <= 0) break;
                _turn++;
            }

            GameEndSceneData.GameResult =
                _playerhp.Health == _enemyhp.Health ? GameResult.Draw :
                _playerhp.Health > _enemyhp.Health ? GameResult.Win : GameResult.Lose;
            SceneManager.LoadScene("GameEnd");
        }
    }
}