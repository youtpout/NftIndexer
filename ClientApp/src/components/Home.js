import React, {  useState, useEffect } from 'react';
import { MetaMaskSDK } from '@metamask/sdk';

const Home = () => {

    const [nfts, setNfts] = useState([]);
    const [owner, setOwner] = useState("");

    const options = {
        injectProvider: true
    };
    const MMSDK = new MetaMaskSDK(options);

    useEffect(() => {

        getNft().then();

    }, []);

    useEffect(() => {

        filterByOwner(owner).then();

    }, [owner]);

    const getNft = async () => {
        const response = await fetch("/api/nft");
        const datas = await response.json();
        setNfts(datas);
    };

    const filterByOwner = async (address) => {
        const response = await fetch("/api/nft/GetWithParameter?owner=" + address);
        const datas = await response.json();
        setNfts(datas);
    };

    const connect = async () => {
        const ethereum = MMSDK.getProvider();
        const accounts = await ethereum.request({ method: 'eth_requestAccounts', params: [] });
        console.log("accounts", accounts);
        if (accounts?.length && accounts[0]?.length) {
            setOwner(accounts[0]);
        } else {
            setOwner("");
        }
    }


    return (
        <div>
            <div className="filter"> <span>Filter by owner : </span>
                <input onChange={ev => setOwner(ev.target.value)} value={owner} type="text" placeholder="owner"></input>
                <button onClick={connect}>Connect Metamask</button>
            </div>
            <div className="home">
                {nfts?.tokens?.map((nft, index) =>
                    <div className="nft-card" key={index}>
                        <AsyncImage {...nft} />

                        <div className="description info">
                            <span title={nft.tokenId}>Token Id : {nft.tokenId?.substring(0, 30) + ((nft.tokenId?.length > 30) ? "..." : "")}</span>
                            <span title={nft.address}>Address :  {nft.address?.substring(0, 30) + ((nft.address?.length > 30) ? "..." : "")}</span>
                            <span title={nft.owner}>Owner : {nft.owner?.substring(0, 30) + ((nft.owner?.length > 30) ? "..." : "")}</span>

                        </div>
                        <MetaDescription {...nft} />

                    </div>)}
                { nfts?.total === 0 && <div>No nft founds for these criterias</div> }
            </div>
        </div>
    );
};

const AsyncImage = (props) => {
    const [loadedSrc, setLoadedSrc] = React.useState(null);
    React.useEffect(() => {
        setLoadedSrc(null);
        if (props.metadatas) {

            const datas = JSON.parse(props.metadatas);
            console.log("metadatas", datas);
            if (datas.image?.length) {
                var url = datas.image.replace("ipfs://", "https://ipfs.io/ipfs/");
                setLoadedSrc({ src: url });

            }
        }
    }, [props.metadatas]);
    if (loadedSrc) {
        return (
            <img alt="image-loaded" {...loadedSrc} />
        );
    }
    return <img alt="no-image" />;
};

const MetaDescription = (props) => {
    const [meta, setMeta] = React.useState(null);
    React.useEffect(() => {
        setMeta(null);
        if (props.metadatas) {

            const datas = JSON.parse(props.metadatas);
            setMeta(datas);
        }
    }, [props.metadatas]);
    if (meta) {
        return (
            <div className="description">
                <span>{meta.name}</span>
                <span title={meta.description}>  {meta.description?.substring(0, 60) + ((meta.description?.length > 60) ? "..." : "")}</span>
            </div>
        );
    }
    return null;
};

export default Home;