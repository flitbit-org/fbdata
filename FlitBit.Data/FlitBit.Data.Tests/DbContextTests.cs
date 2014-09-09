using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using FlitBit.Data.Cluster;
using Moq;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class DbContextTests
    {
        [Test]
        public void DbContext_SharedOrNewConnection_RetrievesConnection()
        {
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                cn.EnsureConnectionIsOpen();
            }
        }

        [Test]
        public void DbContext_SharedOrNewConnection_MultipleCallsRetrieveSameConnection()
        {
            // Only works when the DbProviderHelper indicates the connection may be shared
            // This one is SQL Server with multiple active results sets (MARS).
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);

                cn.EnsureConnectionIsOpen();

                var cn2 = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.AreSame(cn, cn2);
            }
        }

        [Test]
        public void DbContext_SharedOrNewConnection_MultipleCallsRetrieveSameConnection_EvenWhenUpcast()
        {
            // Only works when the DbProviderHelper indicates the connection may be shared
            // This one is SQL Server with multiple active results sets (MARS).
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection("adoWrapper");
                Assert.IsNotNull(cn);

                cn.EnsureConnectionIsOpen();

                var cn2 = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.AreSame(cn, cn2);
            }
        }

        [Test]
        public void DbContext_SharedOrNewConnection_MultipleCallsRetrieveSameConnection_EvenWhenDowncast()
        {
            // Only works when the DbProviderHelper indicates the connection may be shared
            // This one is SQL Server with multiple active results sets (MARS).
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);

                cn.EnsureConnectionIsOpen();

                var cn2 = context.SharedOrNewConnection("adoWrapper");
                Assert.AreSame(cn, cn2);
            }
        }

        [Test]
        public void DbContext_ContextEnsuresConnectionCloses()
        {
            DbConnection cn;
            using (var context = DbContext.NewContext())
            {
                cn = context.NewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);
                cn.EnsureConnectionIsOpen();
            }
            Assert.AreEqual(ConnectionState.Closed, cn.State);
        }

        [Test]
        public void DbContext_ContextEnsuresSharedConnectionCloses()
        {
            DbConnection cn;
            using (var context = DbContext.NewContext())
            {
                cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);
                cn.EnsureConnectionIsOpen();
            }
            Assert.AreEqual(ConnectionState.Closed, cn.State);
        }

        [Test]
        public void DbContext_OnContextOrTransactionCompleted_FiresWhenContextEnds()
        {
            var observedContextEnd = false;
            using (var context = DbContext.NewContext())
            {
                context.OnContextOrTransactionCompleted += (sender, args) =>
                {
                    Assert.IsFalse(args.HasTransactionStatus);
                    Assert.AreEqual(TransactionStatus.Committed, args.TransactionStatus);
                    observedContextEnd = true;
                };

                var cn = context.SharedOrNewConnection("adoWrapper");
                Assert.IsNotNull(cn);
                cn.EnsureConnectionIsOpen();
            }
            Assert.IsTrue(observedContextEnd);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Precondition failed: !IsCompleted")]
        public void DbContext_OnContextOrTransactionCompleted_ThrowsIfSubscribedAfterContextEnd()
        {
            IDbContext context;
            using (context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection("adoWrapper");
                Assert.IsNotNull(cn);
                cn.EnsureConnectionIsOpen();
            }
            context.OnContextOrTransactionCompleted += (sender, args) => { };

            Assert.Fail("Should have blown up.");
        }

        [Test]
        public void DbContext_OnContextOrTransactionCompleted_DelayedUntilAmbientTransactionCompletes()
        {
            var observedContextEnd = false;
            using (new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (var context = DbContext.NewContext())
                {
                    context.OnContextOrTransactionCompleted += (sender, args) =>
                    {
                        Assert.IsTrue(args.HasTransactionStatus);
                        Assert.AreEqual(TransactionStatus.Aborted, args.TransactionStatus,
                            "Should observed the transaction aborted.");
                        observedContextEnd = true;
                    };

                    var cn = context.SharedOrNewConnection("adoWrapper");
                    Assert.IsNotNull(cn);
                    cn.EnsureConnectionIsOpen();

                    Assert.IsFalse(observedContextEnd);
                }
                Assert.IsFalse(observedContextEnd);
                // no transaction completion!
            }
            Assert.IsTrue(observedContextEnd);
        }

        [Test]
        public void DbContext_Put_CanPutWithoutPromotionHandler()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            using (var context = DbContext.NewContext())
            {
                context.Put(item.Key, item.Item, item.Created, null);
            }
        }

        [Test]
        public void DbContext_Put_CanPutWithPromotionHandler_PromotionHandlerIsInvokedOnContextEnd()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            var observedCachePromotion = false;
            var promotionHandler = new Mock<ICachePromotionHandler>();
            SetupPromotionHandlerFor<object>(promotionHandler, (key, it, created) =>
            {
                Assert.AreEqual(item.Key, key);
                Assert.AreEqual(item.Item, it);
                Assert.AreEqual(item.Created, created);
                observedCachePromotion = true;
            });

            using (var context = DbContext.NewContext())
            {
                context.Put(item.Key, item.Item, item.Created, promotionHandler.Object);
            }
            Assert.IsTrue(observedCachePromotion);
        }

        [Test]
        public void DbContext_Put_CanPutWithPromotionHandler_PromotionHandlerIsDelayedUntilTransactionCommits()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            var observedCachePromotion = false;
            var promotionHandler = new Mock<ICachePromotionHandler>();
            SetupPromotionHandlerFor<object>(promotionHandler, (key, it, created) =>
            {
                Assert.AreEqual(item.Key, key);
                Assert.AreEqual(item.Item, it);
                Assert.AreEqual(item.Created, created);
                observedCachePromotion = true;
            });

            using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (var context = DbContext.NewContext())
                {
                    context.Put(item.Key, item.Item, item.Created, promotionHandler.Object);
                    Assert.IsFalse(observedCachePromotion);
                }
                Assert.IsFalse(observedCachePromotion);
                tx.Complete();
            }
            Assert.IsTrue(observedCachePromotion);
        }

        [Test]
        public void DbContext_Put_CanPutWithPromotionHandler_PromotionHandlerDelayedAndDoesntFireWhenTransactionAborts()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            var promotionHandler = new Mock<ICachePromotionHandler>();
            SetupPromotionHandlerFor<object>(promotionHandler, (key, it, created) =>
                                                               Assert.Fail(
                                                                   "Promotion should not happen when transaction aborts."));

            using (new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (var context = DbContext.NewContext())
                {
                    context.Put(item.Key, item.Item, item.Created, promotionHandler.Object);
                }
                // no transaction completion!
            }
        }

        [Test]
        public void DbContext_TryGet_TriesSecondLevelCacheIfNotFoundInContext()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            var secondLevelCacheInvoked = false;
            byte[] outValue;
            var secondLevelCache = new Mock<IClusteredMemory>();
            secondLevelCache.Setup(x => x.TryGet(It.IsAny<string>(), out outValue))
                            .OutCallback((string key, out byte[] v) =>
                            {
                                v = null;
                                secondLevelCacheInvoked = true;
                            })
                            .Returns(false);

            using (var context = DbContext.NewContext())
            {
                context.Put(item.Key, item.Item, item.Created, null);

                object retrieved;

                Assert.IsTrue(context.TryGet(secondLevelCache.Object, item.Key, out retrieved));
                Assert.AreSame(item.Item, retrieved);
                Assert.IsFalse(secondLevelCacheInvoked);

                Assert.IsFalse(context.TryGet(secondLevelCache.Object, "unknown", out retrieved));
                Assert.IsTrue(secondLevelCacheInvoked);
            }
        }

        [Test]
        public void DbContext_Put_CachesItem_CanBeRetrievedFromCache()
        {
            var item = new
            {
                Key = "MyItem",
                Item = new object(),
                Created = true
            };

            using (var context = DbContext.NewContext())
            {
                context.Put(item.Key, item.Item, item.Created, null);

                object retrieved;

                Assert.IsTrue(context.TryGet(null, item.Key, out retrieved));
                Assert.AreSame(item.Item, retrieved);
            }
        }

        void SetupPromotionHandlerFor<T>(Mock<ICachePromotionHandler> promotionHandler,
            Action<string, T, bool> callback)
        {
            promotionHandler.Setup(x => x.PromoteCacheItem(It.IsAny<string>(), It.IsAny<T>(), It.IsAny<bool>()))
                            .Callback(callback);
        }

// ReSharper disable once UnusedMember.Local
        void SetupPromotionHandlerFor<T>(Mock<ICachePromotionHandler> promotionHandler,
            Action<string, TimeSpan, T, bool> callback)
        {
            promotionHandler.Setup(
                x => x.PromoteCacheItem(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<T>(), It.IsAny<bool>()))
                            .Callback(callback);
        }
    }
}