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
        <div className="home">

            {nfts.map((nft, index) =>
                <div className="nft-card" key={index}>
                    <AsyncImage {...nft} />

                    <span title={nft.tokenId}> Token Id : {nft.tokenId?.substring(0, 24) + ((nft.tokenId?.length > 25) ? "..." : "")}</span>
                    <MetaDescription {...nft} />

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
            <img  {...loadedSrc} />
        );
    }
    return null;
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