﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnItUp.Locations;
using TurnItUp.Interfaces;
using Moq;
using TurnItUp.Components;
using System.Collections.Generic;
using Entropy;
using TurnItUp.Pathfinding;
using TurnItUp.Tmx;
using System.Tuples;
using Tests.Factories;
using TurnItUp.Randomization;

namespace Tests.Locations
{
    [TestClass]
    public class LevelFactoryTests
    {
        private World _world;
        private ILevelFactory _levelFactory;
        private Mock<ILevelFactory> _levelFactoryMock;
        private Mock<ILevelRandomizer> _levelRandomizerMock;
        private Mock<ILevel> _levelMock;

        [TestInitialize]
        public void Initialize()
        {
            _world = new World();
            _levelFactory = new LevelFactory();
            _levelRandomizerMock = new Mock<ILevelRandomizer>();
            _levelFactory.LevelRandomizer = _levelRandomizerMock.Object;
            _levelMock = new Mock<ILevel>();
            _levelFactoryMock = new Mock<ILevelFactory>();
            _levelFactoryMock.CallBase = true;
        }

        [TestMethod]
        public void LevelFactory_Construction_IsSuccessful()
        {
            LevelFactory levelFactory = new LevelFactory();

            Assert.IsNotNull(levelFactory.LevelRandomizer);
        }

        [TestMethod]
        public void LevelFactory_InitializingALevelWithDefaultInitializationParams_IsSuccessful()
        {
            LevelInitializationParams initializationParams = new LevelInitializationParams();

            _levelFactory.Initialize(_levelMock.Object, initializationParams);

            // TmxPath NULL - Make sure that the Map is not loaded up and that the characters are not set up
            _levelMock.Verify(l => l.SetUpMap(It.IsAny<string>()),Times.Never());
            _levelMock.Verify(l => l.SetUpCharacters(), Times.Never());
            // Verify that a Pathfinder is set up
            _levelMock.Verify(l => l.SetUpPathfinder(initializationParams.AllowDiagonalMovement));
        }

        [TestMethod]
        public void LevelFactory_InitializingALevelWithAnInitialTiledMap_IsSuccessful()
        {
            LevelInitializationParams initializationParams = new LevelInitializationParams();
            initializationParams.TmxPath = "../../Fixtures/FullExample.tmx";
            initializationParams.AllowDiagonalMovement = true;

            _levelFactory.Initialize(_levelMock.Object, initializationParams);

            // TmxPath NULL - Make sure that the Map is not loaded up and that the characters are not set up
            _levelMock.Verify(l => l.SetUpMap("../../Fixtures/FullExample.tmx"));
            _levelMock.Verify(l => l.SetUpCharacters());
            // Verify that a Pathfinder is set up
            _levelMock.Verify(l => l.SetUpPathfinder(initializationParams.AllowDiagonalMovement));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LevelFactory_RandomizingALevelWithDefaultRandomizationParams_Fails()
        {
            _levelFactory.Randomize(_levelMock.Object, new LevelRandomizationParams());
        }

        [TestMethod]
        public void LevelFactory_RandomizingALevelWithLayerNameAndTileCount_CallsTheCorrectMethodOnTheLevelRandomizer()
        {
            LevelRandomizationParams randomizationParams = new LevelRandomizationParams();
            randomizationParams.LayerName = "Characters";
            randomizationParams.TileCount = 5;

            _levelFactory.Randomize(_levelMock.Object, randomizationParams);
            _levelRandomizerMock.Verify(lr => lr.Randomize(_levelMock.Object, randomizationParams.LayerName, randomizationParams.TileCount.Value));
        }

        [TestMethod]
        public void LevelFactory_RandomizingALevelWithLayerNameTileCountAndTileMaximum_CallsTheCorrectMethodOnTheLevelRandomizer()
        {
            LevelRandomizationParams randomizationParams = new LevelRandomizationParams();
            randomizationParams.LayerName = "Characters";
            randomizationParams.TileCount = 5;
            randomizationParams.TileMaximum = 10;

            _levelFactory.Randomize(_levelMock.Object, randomizationParams);
            _levelRandomizerMock.Verify(lr => lr.Randomize(_levelMock.Object, randomizationParams.LayerName, randomizationParams.TileCount.Value, randomizationParams.TileMaximum.Value + 1));
        }

        [TestMethod]
        public void LevelFactory_CanBuildABlankLevel()
        {
            ILevel level = _levelFactory.BuildLevel(_world);

            Assert.IsNull(level.Map);
            Assert.IsNull(level.CharacterManager);
            Assert.IsNull(level.PathFinder);
            Assert.IsNull(level.TransitionPointManager);
            Assert.IsNull(level.Viewport);
            Assert.AreEqual(_world, level.World);
        }

        [TestMethod]
        public void LevelFactory_GivenInitializationParams_CanBuildAProperlyInitializedLevel()
        {
            LevelInitializationParams initializationParams = new LevelInitializationParams();
            initializationParams.TmxPath = "../../Fixtures/FullExample.tmx";
            initializationParams.AllowDiagonalMovement = true;
            LevelRandomizationParams randomizationParams = new LevelRandomizationParams();
            randomizationParams.LayerName = "Characters";
            randomizationParams.TileCount = 5;

            _levelFactoryMock.Setup(lf => lf.BuildLevel(_world)).Returns(_levelMock.Object);

            _levelFactoryMock.Object.BuildLevel(_world, initializationParams);

            _levelFactoryMock.Verify(lf => lf.Initialize(_levelMock.Object, initializationParams));
            _levelFactoryMock.Verify(lf => lf.Randomize(_levelMock.Object, randomizationParams), Times.Never());
        }

        [TestMethod]
        public void LevelFactory_GivenBothAnInitializationAndRandomizationParams_CanBuildAProperlyInitializedAndRandomizedLevel()
        {
            LevelInitializationParams initializationParams = new LevelInitializationParams();
            initializationParams.TmxPath = "../../Fixtures/FullExample.tmx";
            initializationParams.AllowDiagonalMovement = true;
            LevelRandomizationParams randomizationParams = new LevelRandomizationParams();
            randomizationParams.LayerName = "Characters";
            randomizationParams.TileCount = 5;

            _levelFactoryMock.Setup(lf => lf.BuildLevel(_world)).Returns(_levelMock.Object);

            _levelFactoryMock.Object.BuildLevel(_world, initializationParams, randomizationParams);

            _levelFactoryMock.Verify(lf => lf.Initialize(_levelMock.Object, initializationParams));
            _levelFactoryMock.Verify(lf => lf.Randomize(_levelMock.Object, randomizationParams));
        }
    }
}