import React, { Component, useState, useEffect } from 'react';

const Home = () => {

    const [nfts, setNfts] = useState([]);

    useEffect(() => {

        getNft().then();

    }, []);

    const getNft = async () => {
        const response = await fetch("/api/nft");
        const datas = await response.json();
        setNfts(datas);
    };



    return (
        <div>

            {nfts.map((nft, index) =>
                <div className="nft-card" key={index}>
                    <span title={nft.tokenId}>{nft.tokenId.substring(0, 24) + ((nft.tokenId?.length > 25) ? "..." : "")}</span>
                    <AsyncImage {...nft} />
                </div>)}
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
            <img height="100" {...loadedSrc} />
        );
    }
    return null;
};

export default Home;