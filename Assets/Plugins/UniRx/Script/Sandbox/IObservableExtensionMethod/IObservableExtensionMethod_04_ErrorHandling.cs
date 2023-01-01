using System;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_04_ErrorHandling : MonoBehaviour
    {
        [ContextMenu(nameof(Catch))]
        void Catch()
        {
            {
                // Catchは名前の通りで､例外発生時に､別のIObservableを変換して渡すのに使える
                var subject = new Subject<int>();
                var subscription = subject
                    .Catch((Exception ex) => Observable.Return(int.MaxValue))
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
                // 例外を発生すると､OnErrorの代わりに､変換したIObservableのOnNextで処理してくれる.
                // また､例外を握りつぶした扱いなので､購読はOnCompleted扱いになる
                subject.OnError(new Exception());
                // なので､OnNextで再通知しても､もう飛ばない
                subject.OnNext(0);
            }

            UnityEngine.Debug.Log("====");

            {
                // Catchを例外を握りつぶす､的な用途で使うなら､Observable.Emptyと組わせて使うのがよい.
                // Observable.Emptyは､すぐに完了通知を発行する.
                // なので､以下の例ではOnNextが飛ばない.
                var subject = new Subject<int>();
                var subscription = subject
                    .Catch((Exception ex) => Observable.Empty<int>())
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
                subject.OnError(new Exception());
            }
        }

        [ContextMenu(nameof(Finally))]
        void Finally()
        {
            // Finallyは名前の通りで､例外発生時に必ず呼ばれる処理を定義する.
            // 処理も例外ハンドリングと同じで､必ず最後(OnErrorやOnCompletedの後)に呼ばれる
            var subject = new Subject<int>();
            var subscription = subject
                .Do(i => throw new Exception()) // Rx的な感じで､購読の途中で例外を発生させる
                .Catch((Exception ex) => Observable.Empty<int>())
                .Finally(() => UnityEngine.Debug.Log($"Finally"))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));

            // 適当に通知
            subject.OnNext(0);
        }

        [ContextMenu(nameof(Retry))]
        void Retry()
        {
            {
                // Retryは､OnErrorの握りつぶして､成功するまでリトライする拡張メソッド.
                int retryCount = 0;
                Observable.Create<int>(observer =>
                    {
                        if (retryCount == 3)
                        {
                            observer.OnNext(retryCount);
                            observer.OnCompleted();
                            return Disposable.Empty;
                        }

                        retryCount++;
                        // ※握りつぶされるのはOnErrorで､OnNextの通知自体は握りつぶされない
                        observer.OnNext(retryCount);
                        observer.OnError(new InvalidOperationException());
                        return Disposable.Empty;
                    })
                    // 引数なしだと､成功するまでずっと繰り返す
                    .Retry()
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
            }

            UnityEngine.Debug.Log("====");

            {
                int retryCount = 0;
                Observable.Create<int>(observer =>
                    {
                        if (retryCount == 3)
                        {
                            observer.OnNext(retryCount);
                            observer.OnCompleted();
                            return Disposable.Empty;
                        }

                        retryCount++;
                        observer.OnNext(retryCount);
                        observer.OnError(new InvalidOperationException());
                        return Disposable.Empty;
                    })
                    // 引数でリトライ回数を指定するパターン. こっちの方が一般的か.
                    .Retry(2)
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
            }
        }
    }
}
