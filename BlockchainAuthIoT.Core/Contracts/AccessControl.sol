// SPDX-License-Identifier: MIT
pragma solidity >=0.7.0;

contract AccessControl {

    // A policy that lives on-chain (more expensive but can be used as fallback
    // when remote policies cannot be retrieved for any reason)
    struct OCP {
        string resource; // The name of the remote endpoint or resource
        uint256 startTime; // When this policy becomes valid
        uint256 expiration; // When this policy should be voided (unix epoch)
        mapping (string => bool) boolParams; // e.g. "subresourceAccess" => true
        mapping (string => int) intParams; // e.g. "querySize" => 10
        mapping (string => string) stringParams; // e.g. "environment" => "test"
    }

    // A policy proposal
    struct Proposal {
        bool approved; // Whether the proposal has been approved
        bytes32 hashCode; // The hash of the proposal for verification
        string externalResource; // A link to the policy file to be reviewed
    }

    // An off-chain policy
    struct Policy {
        bytes32 hashCode; // A hash of the proposal
        string externalResource; // A link to the policy file to be reviewed
    }

    // Actors involved
    address payable public owner; // The admin that created the contract
    address[] public admins; // The admins that can propose policy changes
    address public signer; // The signer of this contract

    // State of the contract
    bool public initialized = false; // Whether the contract has been initialized
    uint public price = 0; // The price to pay in order to sign the contract (once initialized)
    uint public amountPaid = 0; // The price paid so far
    bool public signed = false; // Whether the contract has been signed

    // Policies and proposals
    mapping(uint => OCP) public ocps; // The on-chain policies of the contract
    mapping(uint => Policy) public policies; // The off-chain policies of the contract
    mapping(uint => Proposal) public proposals; // The policies proposed by the signer
    
    // Used to generate incremental IDs
    uint public ocpsCount;
    uint public policiesCount;
    uint public proposalsCount;

    // Used for enumeration
    uint public adminsCount; 

    constructor (address signerAddress) {
        // Set the creator as owner and admin
        owner = payable(msg.sender);
        admins.push(msg.sender);
        adminsCount = 1;

        // Set the signer
        signer = signerAddress;
    }

    /*
    =========
    MODIFIERS
    =========
    */

    modifier onlyAdmin {
        bool isAdmin = false;
        for (uint i = 0; i < admins.length; i++) {
            if (msg.sender == admins[i]) {
                isAdmin = true;
                break;
            }
        }
        require (isAdmin, "Not an admin");
        _;
    }

    modifier onlyOwner {
        require (msg.sender == owner, "Not the owner");
        _;
    }

    modifier onlySigner {
        require (msg.sender == signer, "Not the signer");
        _;
    }

    modifier onlyNotInitialized {
        require (!initialized, "Contract already initialized");
        _;
    }

    modifier onlyInitialized {
        require (initialized, "Contract not initialized");
        _;
    }

    modifier onlyNotSigned {
        require (!signed, "Contract already signed");
        _;
    }

    modifier onlySigned {
        require (signed, "Contract not signed");
        _;
    }

    /*
    ===================================
    PUBLIC FUNCTIONS (OWNER and ADMINS)
    ===================================
    */

    function addAdmin (address newAdmin) public onlyAdmin {
        admins.push(newAdmin);
        adminsCount++;
        emit AdminAdded(newAdmin, msg.sender);
    }

    function removeAdmin (address admin) public onlyOwner {
        require (admin != owner, "Cannot remove the owner");
        bool isAdmin = false;
        uint index;
        for (uint i = 0; i < admins.length; i++) {
            if (admin == admins[i]) {
                isAdmin = true;
                index = i;
                break;
            }
        }

        require (isAdmin, "Admin not found");

        delete admins[index];
        emit AdminRemoved(admin);
    }

    function initializeContract (uint contractPrice) public onlyAdmin onlyNotInitialized {
        price = contractPrice;
        initialized = true;
        emit Initialized();
    }

    // Only admins are able to add off-chain and on-chain policies,
    // when the contract is not yet initialized
    function createPolicy (bytes32 hashCode, string memory externalResource) 
        public onlyAdmin onlyNotInitialized {
            uint policyId = policiesCount++;
            Policy storage newPolicy = policies[policyId];
            newPolicy.hashCode = hashCode;
            newPolicy.externalResource = externalResource;
    }

    function createOCP (string memory resource, uint256 startTime, uint256 expiration)
        public onlyAdmin onlyNotInitialized {
            uint ocpId = ocpsCount++;
            OCP storage ocp = ocps[ocpId];
            ocp.resource = resource;
            ocp.startTime = startTime;
            ocp.expiration = expiration;

            emit PolicyAdded(ocpId);
    }

    function setOCPBoolParam (uint ocpId, string memory name, bool value)
        public onlyAdmin onlyNotInitialized {
            OCP storage ocp = ocps[ocpId];
            ocp.boolParams[name] = value;
    }

    function setOCPIntParam (uint ocpId, string memory name, int value)
        public onlyAdmin onlyNotInitialized {
            OCP storage ocp = ocps[ocpId];
            ocp.intParams[name] = value;
    }

    function setOCPStringParam (uint ocpId, string memory name,
        string memory value) public onlyAdmin onlyNotInitialized {
            OCP storage ocp = ocps[ocpId];
            ocp.stringParams[name] = value;
    }

    // Admins can approve proposals and promote them to policies
    function approveProposal (uint proposalId) public onlyAdmin onlySigned {
        Proposal storage proposal = proposals[proposalId];
        require (!proposal.approved, "Proposal already approved");
        proposal.approved = true;

        uint policyId = policiesCount++;
        Policy storage policy = policies[policyId];
        policy.hashCode = proposal.hashCode;
        policy.externalResource = proposal.externalResource;

        emit ProposalApproved(proposalId, policyId);
    }

    /*
    =========================
    PUBLIC FUNCTIONS (SIGNER)
    =========================
    */

    // The signer can sign the contract (by sending enough money to cover the price)
    function signContract () public onlySigner onlyInitialized onlyNotSigned payable {
        amountPaid += msg.value;

        // If the signer sent enough money, sign the contract and transfer the money to the owner
        // TODO: Optionally refund the difference to the signer
        if (amountPaid >= price) {
            owner.transfer(price);
            signed = true;
            emit Signed();
        }
    }

    // The signer is able to create proposals to request access to additional
    // resources (and expand the contract). An admin may then approve the
    // proposal once it's finalized
    function createProposal (bytes32 hashCode, string memory externalResource) 
        public onlySigner onlySigned {
            uint proposalId = proposalsCount++;
            Proposal storage newProposal = proposals[proposalId];
            newProposal.approved = false;
            newProposal.hashCode = hashCode;
            newProposal.externalResource = externalResource;
    }

    /*
    ===========================
    PUBLIC FUNCTIONS (EVERYONE)
    ===========================
    */

    // Get parameter values of on-chain policies
    function getOCPBoolParam (uint ocpId, string memory name) public view
        returns (bool value) {
        OCP storage ocp = ocps[ocpId];
        value = ocp.boolParams[name];
    }

    function getOCPIntParam (uint ocpId, string memory name) public view
        returns (int value) {
        OCP storage ocp = ocps[ocpId];
        value = ocp.intParams[name];
    }

    function getOCPStringParam (uint ocpId, string memory name) public view
        returns (string memory value) {
        OCP storage ocp = ocps[ocpId];
        value = ocp.stringParams[name];
    }

    /*
    ======
    EVENTS
    ======
    */

    event AdminAdded (address admin, address sender);
    event AdminRemoved (address admin);
    event Initialized ();
    event Signed ();
    event PolicyAdded (uint policyId);
    event ProposalApproved (uint proposalId, uint policyId);
}