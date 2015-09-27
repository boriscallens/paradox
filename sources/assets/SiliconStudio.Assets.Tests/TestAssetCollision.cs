﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetCollision
    {
        [Test]
        public void TestSimple()
        {
            var inputs = new AssetItemCollection();

            var asset = new AssetObjectTest();
            for (int i = 0; i < 10; i++)
            {
                var newAsset = new AssetObjectTest() { Id = asset.Id, Reference =  new AssetReference<AssetObjectTest>(asset.Id, "bad")};
                inputs.Add(new AssetItem("0", newAsset));
            }

            // Tries to use existing ids
            var outputs = new AssetItemCollection();
            AssetCollision.Clean(null, inputs, outputs, new AssetResolver(), true);

            // Make sure we are generating exactly the same number of elements
            Assert.AreEqual(inputs.Count, outputs.Count);

            // Make sure that asset has been cloned
            Assert.AreNotEqual(inputs[0], outputs[0]);

            // First Id should not change
            Assert.AreEqual(inputs[0].Id, outputs[0].Id);

            // Make sure that all ids are different
            var ids = new HashSet<Guid>(outputs.Select(item => item.Id));
            Assert.AreEqual(inputs.Count, ids.Count);

            // Make sure that all locations are different
            var locations = new HashSet<UFile>(outputs.Select(item => item.Location));
            Assert.AreEqual(inputs.Count, locations.Count);

            // Reference location "bad"should be fixed to "0"
            foreach (var output in outputs)
            {
                var assetRef = ((AssetObjectTest)output.Asset).Reference;
                Assert.AreEqual("0", assetRef.Location);
                Assert.AreEqual(outputs[0].Id, assetRef.Id);
            }
        }

        [Test]
        public void TestSimpleNewGuids()
        {
            var inputs = new AssetItemCollection();

            var asset = new AssetObjectTest();
            for (int i = 0; i < 10; i++)
            {
                var newAsset = new AssetObjectTest() { Id = asset.Id, Reference = new AssetReference<AssetObjectTest>(asset.Id, "bad") };
                inputs.Add(new AssetItem("0", newAsset));
            }

            // Force to use new ids
            var outputs = new AssetItemCollection();
            AssetCollision.Clean(null, inputs, outputs, new AssetResolver() { AlwaysCreateNewId = true }, true);

            // Make sure we are generating exactly the same number of elements
            Assert.AreEqual(inputs.Count, outputs.Count);

            // Make sure that asset has been cloned
            Assert.AreNotEqual(inputs[0], outputs[0]);

            // First Id should not change
            Assert.AreNotEqual(inputs[0].Id, outputs[0].Id);

            // Make sure that all ids are different
            var ids = new HashSet<Guid>(outputs.Select(item => item.Id));
            Assert.AreEqual(inputs.Count, ids.Count);

            // Make sure that all locations are different
            var locations = new HashSet<UFile>(outputs.Select(item => item.Location));
            Assert.AreEqual(inputs.Count, locations.Count);

            // Reference location "bad"should be fixed to "0"
            foreach (var output in outputs)
            {
                var assetRef = ((AssetObjectTest)output.Asset).Reference;
                Assert.AreEqual("0", assetRef.Location);
                Assert.AreEqual(outputs[0].Id, assetRef.Id);
            }
        }

        [Test]
        public void TestWithPackage()
        {
            var inputs = new AssetItemCollection();

            var asset = new AssetObjectTest();

            var package = new Package();
            package.Assets.Add(new AssetItem("0", asset));

            for (int i = 0; i < 10; i++)
            {
                var newAsset = new AssetObjectTest() { Id = asset.Id, Reference = new AssetReference<AssetObjectTest>(asset.Id, "bad") };
                inputs.Add(new AssetItem("0", newAsset));
            }

            // Tries to use existing ids
            var outputs = new AssetItemCollection();
            AssetCollision.Clean(null, inputs, outputs, AssetResolver.FromPackage(package), true);

            // Make sure we are generating exactly the same number of elements
            Assert.AreEqual(inputs.Count, outputs.Count);

            // Make sure that asset has been cloned
            Assert.AreNotEqual(inputs[0], outputs[0]);

            // First Id should not change
            Assert.AreNotEqual(inputs[0].Id, outputs[0].Id);

            // Make sure that all ids are different
            var ids = new HashSet<Guid>(outputs.Select(item => item.Id));
            Assert.AreEqual(inputs.Count, ids.Count);

            // Make sure that all locations are different
            var locations = new HashSet<UFile>(outputs.Select(item => item.Location));
            Assert.AreEqual(inputs.Count, locations.Count);

            // Reference location "bad"should be fixed to "0_1" pointing to the first element
            foreach (var output in outputs)
            {
                // Make sure of none of the locations are using "0"
                Assert.AreNotEqual("0", output.Location);

                var assetRef = ((AssetObjectTest)output.Asset).Reference;
                Assert.AreEqual("0 (2)", assetRef.Location);
                Assert.AreEqual(outputs[0].Id, assetRef.Id);
            }
        }
    }
}