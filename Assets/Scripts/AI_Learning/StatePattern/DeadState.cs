using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// DEAD 상태 - 마지막 상태.
    ///
    /// Enter   : 쓰러지는 표현을 하고 충돌을 끈다.
    /// Execute : 아무것도 하지 않는다.
    /// 전환    : 없다. (다른 상태로 나가지 않는다)
    ///
    /// 학습 포인트: 모든 상태가 다른 상태로 이어지는 것은 아니다.
    /// 나가는 화살표가 없는 상태를 최종 상태(terminal state) 라고 부른다.
    /// </summary>
    public class DeadState : AIState
    {
        public DeadState(EnemyStateMachine machine) : base(machine) { }

        public override string Name => "DEAD";
        public override string Description => "HP 가 0 이 되어 더 이상 어떤 상태로도 전환되지 않는다. (최종 상태)";

        public override void Enter()
        {
            Ctx.TickDead();

            // 쓰러진 것처럼 보이게 회전시키고 반투명하게 만든다.
            Ctx.transform.rotation = Quaternion.Euler(0f, 0f, -80f);

            SpriteRenderer sr = Ctx.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.45f;
                sr.color = c;
            }

            Collider2D col = Ctx.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        public override void Execute()
        {
            // 죽었으므로 아무 행동도 하지 않는다.
            Ctx.TickDead();
        }
    }
}
