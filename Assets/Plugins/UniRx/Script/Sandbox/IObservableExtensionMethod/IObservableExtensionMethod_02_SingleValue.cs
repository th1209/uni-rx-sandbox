using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_02_SingleValue : MonoBehaviour
    {
        [ContextMenu(nameof(First))]
        void First()
        {
            {
                // Firstは､IObservableの通知を最初のもののみにフィルタする
                var subscription = Observable.Range(1, 10)
                    .First()
                    .Subscribe(i => UnityEngine.Debug.Log($"Take first value. value:{i}"));
            }

            try
            {
                // Firstは､LINQと同じで､最初の通知取れなければ例外を送出する.
                var subscription = Observable.Range(1, 0)
                    .First()
                    .Subscribe(i => UnityEngine.Debug.Log($"Take first value. value:{i}"));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"ex:{e}");
            }

            {
                // 上記を考慮する場合は､LINQと同じくFirstOrDefaultを使う.
                // この場合､デフォルト値0で一回通知される
                var subscription = Observable.Range(1, 0)
                    .FirstOrDefault()
                    .Subscribe(i => UnityEngine.Debug.Log($"Fail safely. value:{i}"));
            }

            // 標準のReactiveExtensionsだと､以下のような例が紹介されていて､Firstがブロッキングな動作をするとのこと.UniRxの場合はそんなこと無かった.
            // また､標準のReactiveExtensionsだと､Firstは値を返す模様. UniRxの場合は､Firstの場合もIObservableを返す
            // UnityEngine.Debug.Log($"Start {DateTime.Now:yyyy/MM/dd HH:mm:ss.FFF}");
            // var firstResult = Observable.Interval(TimeSpan.FromMilliseconds(2000))
            //     .First();
            // UnityEngine.Debug.Log($"End: {DateTime.Now:yyyy/MM/dd HH:mm:ss.FFF}");
            // UnityEngine.Debug.Log($"Result: {firstResult}");
        }

        [ContextMenu(nameof(Last))]
        void Last()
        {
            {
                var subscription = Observable.Range(1, 10)
                    .Last()
                    .Subscribe(i => UnityEngine.Debug.Log($"Take last value. value:{i}"));
            }

            try
            {
                var subscription = Observable.Range(1, 0)
                    .Last()
                    .Subscribe(i => UnityEngine.Debug.Log($"Take last value. value:{i}"));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"ex:{e}");
            }

            {
                var subscription = Observable.Range(1, 0)
                    .LastOrDefault()
                    .Subscribe(i => UnityEngine.Debug.Log($"Fail safely. value:{i}"));
            }
        }


        [ContextMenu(nameof(Single))]
        void Single()
        {
            // Singleは､Firstとほぼ同じなのだが､"通知が必ず1件しかないこと"のケースで使われる.
            {
                var subscription = Observable.Return(0)
                    .Single()
                    .Subscribe(i => UnityEngine.Debug.Log($"Take single value. value:{i}"));
            }

            try
            {
                // 以下のように､複数回通知が来るケースでは､Singleだと例外になってしまう
                var subscription = Observable.Range(1, 10)
                    //.Single()
                    .SingleOrDefault() // SingleOrDefaultでも､複数通知が飛ぶ場合は同じく例外が発生する
                    .Subscribe(i => UnityEngine.Debug.Log($"Take single value. value:{i}"));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"ex:{e}");
            }
        }

        [ContextMenu(nameof(Skip))]
        void Skip()
        {
            // Skipは､通知から先頭3件を読み飛ばす
            var subscription = Observable.Range(0, 10)
                .Skip(3)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
        }

        [ContextMenu(nameof(Take))]
        void Take()
        {
            // Takeは､通知から先頭3件を取得する
            var subscription = Observable.Range(0, 10)
                .Take(3)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
        }

        [ContextMenu(nameof(TakeLast))]
        void TakeLast()
        {
            // TakeLastは､通知から後ろ3件を取得する
            // (何故か対になるSkipLastはUniRxでは未実装な模様)
            var subscription = Observable.Range(0, 10)
                .TakeLast(3)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
        }

        [ContextMenu(nameof(Skip_Take_Repeat))]
        void Skip_Take_Repeat()
        {
            // Repeatは､それまでの操作を繰り替えす拡張メソッド.
            // 以下のようにSkip/Takeと組み合わせると､3個飛ばして3個取って...をずっと繰り返す
            var subject = new Subject<int>();
            var subscription = subject
                .Skip(3)
                .Take(3)
                .Repeat()
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
            for (int i = 0; i < 20; i++)
            {
                subject.OnNext(i);
            }
        }

        [ContextMenu(nameof(SkipWhile))]
        void SkipWhile()
        {
            // SkipWhileは､指定した条件の間は飛ばす.
            var subscription = Observable.Range(0, 10)
                .SkipWhile(i => i < 5)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
        }

        [ContextMenu(nameof(TakeWhile))]
        private void TakeWhile()
        {
            var subscription = Observable.Range(0, 10)
                .TakeWhile(i => i < 5)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));
        }

        [ContextMenu(nameof(SkipUntil))]
        void SkipUntil()
        {
            var startTrigger = new Subject<Unit>();

            var subject = new Subject<int>();
            var subscription = subject
                // SkipUntilは､こういう感じで別のIObservableを引数に取る. 別のObservableから通知があるまでずっとスキップされる
                .SkipUntil(startTrigger)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));

            // つまり､こっちは発火されなくて...
            UnityEngine.Debug.Log("==== OnNext Start ====");
            for (int i = 0; i < 5; i++)
            {
                subject.OnNext(i);
            }

            UnityEngine.Debug.Log("==== OnNext End ====");

            // こっちは起動条件を満たしたので発火する!
            startTrigger.OnNext(Unit.Default);
            UnityEngine.Debug.Log("==== OnNext Start(StartTriggerInvoked) ====");
            for (int i = 0; i < 5; i++)
            {
                subject.OnNext(i);
            }

            UnityEngine.Debug.Log("==== OnNext End(StartTriggerInvoked) ====");
        }

        [ContextMenu(nameof(TakeUntil))]
        void TakeUntil()
        {
            var endTrigger = new Subject<Unit>();

            var subject = new Subject<int>();
            var subscription = subject
                .TakeUntil(endTrigger)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i})"));

            // つまり､こっちは発火されて...
            UnityEngine.Debug.Log("==== OnNext Start ====");
            for (int i = 0; i < 5; i++)
            {
                subject.OnNext(i);
            }

            UnityEngine.Debug.Log("==== OnNext End ====");

            // こっちは終了条件を満たしたのでもう発火しない!
            endTrigger.OnNext(Unit.Default);
            UnityEngine.Debug.Log("==== OnNext Start(EndTriggerInvoked) ====");
            for (int i = 0; i < 5; i++)
            {
                subject.OnNext(i);
            }

            UnityEngine.Debug.Log("==== OnNext End(EndTriggerInvoked) ====");
        }
    }
}
