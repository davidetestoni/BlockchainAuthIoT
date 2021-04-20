#!/bin/bash
rm -rf devChain/geth
./geth --datadir=devChain init genesis_clique.json
./geth --nodiscover --datadir=devChain --http --http.port 8545 --http.addr 0.0.0.0 --http.corsdomain '*' --http.vhosts '*' --http.api 'eth,web3,personal,net,miner,admin,debug' --mine --allow-insecure-unlock --unlock 0x12890d2cce102216644c59daE5baed380d84830c --password "pass.txt" --miner.gastarget 16234336 --miner.gaslimit 16234336 --verbosity 0 console