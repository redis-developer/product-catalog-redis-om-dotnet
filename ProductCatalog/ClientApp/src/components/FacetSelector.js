import React, {useState} from 'react';
import {Button, Input} from "reactstrap";

const FacetSelector = ({facetName, facetItems, updateFilterCallback})=>{
    facetItems = facetItems.filter(str=> str !== "");
    const [facets, setFacets] = useState([]);
    const [hideFacet, setHideFacet] = useState(false);
    const handleFacetUpdate = async (event, idx) => {
        const newFacets = [...facets]
        if(event.target.checked){
            newFacets.push(facetItems[idx]);
        } else{
            newFacets.splice(newFacets.indexOf(facetItems[idx]), 1);            
        }
        
        setFacets(newFacets);        
        await updateFilterCallback(facetName, newFacets);
    }
    
    return(
        <div style={{textAlign: "left", paddingTop: "10px", paddingBottom: "10px"}}>
            <Button style={{width:"100%"}} className="btn-group-sm" onClick={()=>setHideFacet(!hideFacet)}>
                {facetName}
            </Button>
            {hideFacet &&(
                <div>
                    {facetItems.filter(str=>str !=="").map((item, idx) =>(
                        <label style={{display: "block"}} key={idx}>
                            <Input
                                type="checkbox"
                                value={item}
                                checked={facets.indexOf(item) !== -1}
                                onChange={async (e)=> await handleFacetUpdate(e, idx)}
                            />
                            {item}
                        </label>
                    ))}
                </div>
            )}
        </div>
    )
}

export default FacetSelector